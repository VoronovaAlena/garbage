using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

using System.Linq;
using System.IO;
using Data;
using System.Collections.Generic;
using Garbage.Frameworks;

namespace Garbage
{
    /// <summary>Логика взаимодействия для MainWindow.xaml</summary>
    public partial class MainWindow : Window
    {

		#region .ctor

		public MainWindow()
        {
            InitializeComponent();

            _storyboard.CurrentTimeInvalidated += OnStoryboardTimeInvalidated;
            _storyboard.Children.Add(_mediaTimeline);
        }

        #endregion

        #region Data

        private bool go_player;


        Storyboard _storyboard       = new Storyboard();
        MediaTimeline _mediaTimeline = new MediaTimeline();
        IClient _client              = new ClientPostRequestForFlask();

		#endregion

		#region Handlers

		private void Play_Click(object sender, RoutedEventArgs e)
        {
            _storyboard.Resume(Video);
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            _storyboard.Pause(Video);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _storyboard.Stop(Video);
            _mediaTimeline.Source = null;
        }

        private void OnOpenFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Multiselect = false, Filter = "All Media Files|*.wav;*.aac;*.wma;*.wmv;*.avi;*.mpg;*.mpeg;*.m1v;*.mp2;*.mp3;*.mpa;*.mpe;*.m3u;*.mp4;*.mov;*.3g2;*.3gp2;*.3gp;*.3gpp;*.m4a;*.cda;*.aif;*.aifc;*.aiff;*.mid;*.midi;*.rmi;*.mkv;*.WAV;*.AAC;*.WMA;*.WMV;*.AVI;*.MPG;*.MPEG;*.M1V;*.MP2;*.MP3;*.MPA;*.MPE;*.M3U;*.MP4;*.MOV;*.3G2;*.3GP2;*.3GP;*.3GPP;*.M4A;*.CDA;*.AIF;*.AIFC;*.AIFF;*.MID;*.MIDI;*.RMI;*.MKV" };
            if(openFileDialog.ShowDialog() == true)
            {
                _mediaTimeline.Source = new Uri(openFileDialog.FileName);
                _storyboard.Begin(Video, true);
            }
        }

        /// <summary>Ловим обновление.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStoryboardTimeInvalidated(object sender, EventArgs e)
        {
            Clock storyboardClock = (Clock)sender;

            if(storyboardClock.CurrentProgress != null)
            {
                go_player = true;
                TimerSlider.Value = storyboardClock.CurrentTime.Value.TotalSeconds;
                go_player = false;

                var image = GetCrop(Video);
                if(image != null)
                {
                    var ser = SerilizationImageSource(image);
                    if(ser is not null)
                    {
                        //Запрос на flask сервер.
                        var response = _client.Send("api", ser);
                        var rsp = response.Content.ReadAsStringAsync().Result;

                        var result = JsonParse.Deserialize<List<object>>(rsp);

                        foreach(var item in result)
                        {
                            if(item is not double)
                            {
                                var binaryDatas = JsonParse.Deserialize<BinaryData>(item.ToString());
                                if(binaryDatas != null && binaryDatas.boxes.Length > 0)
                                {
                                    var box = binaryDatas.boxes;

                                    Array pixels = null; ;
                                    var cache = new CachedBitmap(image, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                                    image.Decoder.Preview.CopyPixels(pixels, 0, 0);

                                    
                                }
                            }
                        }
                    }
                }
                mainImage.Source = image;
            }
        }

        private void Image_MediaOpened(object sender, RoutedEventArgs eventArgs)
        {

        }

        private void TimerSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _storyboard.Pause(Video);
        }

        private void TimerSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _storyboard.Seek(Video, TimeSpan.FromSeconds(TimerSlider.Value), TimeSeekOrigin.BeginTime);
            _storyboard.Resume(Video);
        }

        private void TimerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(!go_player)
                _storyboard.Seek(Video, TimeSpan.FromSeconds(TimerSlider.Value), TimeSeekOrigin.BeginTime);
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

        private void Video_MediaOpened(object sender, RoutedEventArgs e)
        {
            TimerSlider.Maximum = Video.NaturalDuration.TimeSpan.TotalSeconds;
        }

		#endregion

		#region Methods

        /// <summary>Получает изображение из UI элемента.</summary>
        /// <param name="elt"></param>
        /// <returns></returns>
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

        /// <summary>Конвертация в байты.</summary>
        /// <param name="bitmapsource"></param>
        /// <returns></returns>
        private byte[] SerilizationImageSource(BitmapSource bitmapsource)
        {
            using(MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                return outStream.ToArray();
            }
        }


        #endregion

	}
}
