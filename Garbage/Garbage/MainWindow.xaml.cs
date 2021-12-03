using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

using System.Linq;

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
            OpenFileDialog openFileDialog = new OpenFileDialog() { Multiselect = false, Filter = "All Media Files|*.wav;*.aac;*.wma;*.wmv;*.avi;*.mpg;*.mpeg;*.m1v;*.mp2;*.mp3;*.mpa;*.mpe;*.m3u;*.mp4;*.mov;*.3g2;*.3gp2;*.3gp;*.3gpp;*.m4a;*.cda;*.aif;*.aifc;*.aiff;*.mid;*.midi;*.rmi;*.mkv;*.WAV;*.AAC;*.WMA;*.WMV;*.AVI;*.MPG;*.MPEG;*.M1V;*.MP2;*.MP3;*.MPA;*.MPE;*.M3U;*.MP4;*.MOV;*.3G2;*.3GP2;*.3GP;*.3GPP;*.M4A;*.CDA;*.AIF;*.AIFC;*.AIFF;*.MID;*.MIDI;*.RMI;*.MKV" };
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

        private BitmapFrame GetCrop(UIElement elt)
        {
            double h = elt.RenderSize.Height;
            double w = elt.RenderSize.Width;
            if(h > 0)
            {
                PresentationSource source = PresentationSource.FromVisual(elt);
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)w, (int)h, 96, 96, PixelFormats.Default);

                VisualBrush sourceBrush = new VisualBrush(elt);
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                using(drawingContext)
                {
                    drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0),
                          new Point(w, h)));
                }
                rtb.Render(drawingVisual);

                // return rtb;
                var encoder = new PngBitmapEncoder();
                var outputFrame = BitmapFrame.Create(rtb);
                encoder.Frames.Add(outputFrame);

                return encoder.Frames.FirstOrDefault();
            }
            return null;
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

                var image = GetCrop(Video);
                mainImage.Source = image;
            }
        }

        private void Image_MediaOpened(object sender, RoutedEventArgs eventArgs)
        { 
            
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

		private void OpenRTSP_Click(object sender, RoutedEventArgs e)
		{
            RTSPOpenWindow window = new RTSPOpenWindow();
            window.ShowDialog();

            if(window.RTSP.Text != "")
			{
                Video.Source = new Uri(window.RTSP.Text);
			}
        }
	}
}
