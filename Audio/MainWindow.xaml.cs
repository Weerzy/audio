using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Media;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;


namespace Audio
{
    public partial class MainWindow : Window
    {
        private string[] supportedExtensions = {".mp3", ".wav", ".m4a"};
        private List<string> playlist;
        private int CurrentTrack;
        private bool playing = false;
        private bool repeat = false;
        private bool shuffle = false;
        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
        }
        private async void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string folderPath = dialog.FileName;
                await LoadTrack(folderPath);
                PlayCurrentTrack();
            }
            CurrentTime.Visibility = Visibility.Visible;
            FinalTime.Visibility = Visibility.Visible;
        }
        private async Task LoadTrack(string folderPath)
        {
            string[] files = Directory.GetFiles(folderPath);
            playlist = files.Where(file => supportedExtensions.Contains(System.IO.Path.GetExtension(file).ToLower())).ToList();
            TrackList.ItemsSource = playlist.Select(file => System.IO.Path.GetFileName(file));
        }
        private void PlayCurrentTrack()
        {
            if (playlist != null && playlist.Count > 0 && CurrentTrack >= 0 && CurrentTrack < playlist.Count)
            {
                string trackPath = playlist[CurrentTrack];
                MediaElement.Source = new Uri(trackPath);
                MediaElement.Play();
                playing = true;
                UpdatePlayPauseButton();
            }
        }
        private void UpdatePlayPauseButton()
        {
            if (playing)
            {
                PlayPauseButton.Content = "Pause";
            }
            else
            {
                PlayPauseButton.Content = "Play";
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentTrack--;
            if (CurrentTrack < 0)
            {
                CurrentTrack = playlist.Count - 1;
            }
            PlayCurrentTrack();
        }
        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (playing)
            {
                MediaElement.Pause();
                playing = false;
            }
            else
            {
                MediaElement.Play();
                playing = true;
            }
            UpdatePlayPauseButton();
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentTrack++;
            if (CurrentTrack >= playlist.Count)
            {
                CurrentTrack = 0;
            }
            PlayCurrentTrack();
        }
        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            repeat = !repeat;
            if (repeat)
            {
                RepeatButton.Background = Brushes.LightGray;
            }
            else
            {
                RepeatButton.Background = Brushes.Transparent;
            }
        }
        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            shuffle = !shuffle;
            if (shuffle)
            {
                ShuffleButton.Background = Brushes.LightGray;
                ShufflePlaylist();
            }
            else
            {
                ShuffleButton.Background = Brushes.Transparent;
                SortPlaylist();
            }
        }
        private void ShufflePlaylist()
        {
            Random random = new Random();
            int n = playlist.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                string value = playlist[k];
                playlist[k] = playlist[n];
                playlist[n] = value;
            }
        }
        private void SortPlaylist()
        {
            playlist.Sort();
        }
        private void TrackList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentTrack = TrackList.SelectedIndex;
            PlayCurrentTrack();
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MediaElement.NaturalDuration.HasTimeSpan)
            {
                TimeSpan newPosition = TimeSpan.FromSeconds(e.NewValue);
                MediaElement.Position = newPosition;
            }
        }
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (MediaElement.NaturalDuration.HasTimeSpan)
            {
                TimeSpan duration = MediaElement.NaturalDuration.TimeSpan;
                Slider.Maximum = duration.TotalSeconds;
                FinalTime.Text = TimeFormat(duration);
            }
        }
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (repeat)
            {
                PlayCurrentTrack();
            }
            else
            {
                NextButton_Click(sender, e);
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (MediaElement.NaturalDuration.HasTimeSpan && !Slider.IsMouseCaptureWithin)
            {
                TimeSpan currentTime = MediaElement.Position;
                TimeSpan totalTime = MediaElement.NaturalDuration.TimeSpan;
                TimeSpan remainingTime = totalTime - currentTime;

                CurrentTime.Text = TimeFormat(currentTime);
                FinalTime.Text = "-" + TimeFormat(remainingTime);

                Slider.Value = currentTime.TotalSeconds;
            }
        }

        private string TimeFormat(TimeSpan time)
        {
            return $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        private void InitializeTimer()
        {
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += UpdateTimer_Tick;
            timer.Start();
        }
    }
}