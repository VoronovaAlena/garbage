using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Garbage
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            storyboard.CurrentTimeInvalidated += Storyboard_CurrentTimeInvalidated;
            storyboard.Children.Add(mediaTimeline);
        }

        Storyboard storyboard = new Storyboard();
        MediaTimeline mediaTimeline = new MediaTimeline();

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            storyboard.Resume(Video);
        }
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            storyboard.Pause(Video);
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            storyboard.Stop(Video);
            mediaTimeline.Source = null;
        }
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Multiselect = false, Filter = "MP4 File|*.mp4|All File|*.*" };
            if(openFileDialog.ShowDialog() == true)
            {
                mediaTimeline.Source = new Uri(openFileDialog.FileName);
                storyboard.Begin(Video, true);
            }
        }

        // При открытии файла максимум ползунка ставим в зависимости от длительности видео
        private void Video_MediaOpened(object sender, RoutedEventArgs e)
        {
            TimerSlider.Maximum = Video.NaturalDuration.TimeSpan.TotalSeconds;
        }


        private bool go_player;

        private void Storyboard_CurrentTimeInvalidated(object sender, EventArgs e)
        {

            //Получаем значение времени для раскадровки
            Clock storyboardClock = (Clock)sender;

            if(storyboardClock.CurrentProgress != null)
            {
                go_player = true;
                TimerSlider.Value = storyboardClock.CurrentTime.Value.TotalSeconds;
                go_player = false;
            }
        }

        private void TimerSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            storyboard.Pause(Video);
        }

        private void TimerSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            storyboard.Seek(Video, TimeSpan.FromSeconds(TimerSlider.Value), TimeSeekOrigin.BeginTime);
            storyboard.Resume(Video);
        }

        private void TimerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(!go_player)
                storyboard.Seek(Video, TimeSpan.FromSeconds(TimerSlider.Value), TimeSeekOrigin.BeginTime);
        }

		private void Image_MediaOpened(object sender, RoutedEventArgs e)
		{

		}
	}
}
