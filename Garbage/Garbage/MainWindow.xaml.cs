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
using Garbage.Data;

namespace Garbage
{
	/// <summary>Логика взаимодействия для MainWindow.xaml</summary>
	public partial class MainWindow : Window
	{
		IEnumerable<string> Modules = new List<string>()
		{
			"Детектор заполненности мусорных баков",
			"Детектор мусора",
			"Уборка снега",
			"Обнаружение вандализма",
			"Стоянка автомобилей на газоне",
		};

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


		Storyboard _storyboard = new Storyboard();
		MediaTimeline _mediaTimeline = new MediaTimeline();
		IClient _client = new ClientPostRequestForFlask();

		#endregion

		#region Handlers

		private void Play_Click(object sender, RoutedEventArgs e)
		{
			_storyboard.Seek(Video, TimeSpan.FromSeconds(TimerSlider.Value), TimeSeekOrigin.BeginTime);
			_storyboard.Resume(Video);
		}

		private void Pause_Click(object sender, RoutedEventArgs e)
		{
			_storyboard.Pause(Video);
		}

		private void Stop_Click(object sender, RoutedEventArgs e)
		{
			TimerSlider.Value = 0;
			_storyboard.Stop(Video);
			_storyboard.Seek(TimeSpan.Zero,TimeSeekOrigin.BeginTime);
			_mediaTimeline.Source = null;
		}

		private void OpenImage_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog() { Multiselect = false, Filter = "All Media Files|*png;*jpg" };
			if(openFileDialog.ShowDialog() == true)
			{
				var image = new BitmapImage(new Uri(openFileDialog.FileName));
				if(image != null)
				{
					var ser = SerilizationImageSource(image);
					Detect(ser, image);
				}
			}
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
					Detect(ser, image, true);
				}
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

			try
			{
				if(window.RTSP.Text != "")
				{
					Video.Source = new Uri(window.RTSP.Text);
				}
			}
			catch (Exception exc)
			{
				Log.Text = $"{exc}()\n Соединение по RTSP не удалось.";
			}
		}

		private void Video_MediaOpened(object sender, RoutedEventArgs e)
		{
			TimerSlider.Maximum = Video.NaturalDuration.TimeSpan.TotalSeconds;
		}

		#endregion

		#region Methods

		/// <summary>Метод распознавания.</summary>
		/// <param name="ser">Картинка, отправляемая на сервер.</param>
		/// <param name="image">Исходное изображение.</param>
		/// <param name="locked">Блокирование пустых кадров.</param>
		private void Detect(byte[] ser, BitmapSource image, bool locked = false)
		{
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
						if(binaryDatas != null && binaryDatas.boxes.Count > 0)
						{
							var boxes = binaryDatas.boxes;

							var imageR = GetPixels(image);
							var color = new PixelData()
							{
								Alpha = 255,
								Blue = 0,
								Green = 0,
								Red = 255
							};

							foreach(var box in boxes)
							{
								var minX = Math.Max(box[0] - box[2] / 2, 0);
								var maxX = Math.Min(box[0] + box[2] / 2, (int)image.PixelWidth - 1);

								var minY = Math.Max(box[1] - box[3] / 2, 0);
								var maxY = Math.Min(box[1] + box[3] / 2, (int)image.PixelHeight - 1);

								for(int i = minX; i < maxX; i++)
								{
									imageR[i, maxY] = color;
									imageR[i, minY] = color;
								}
								for(int i = minY; i < maxY; i++)
								{
									imageR[minX, i] = color;
									imageR[maxX, i] = color;
								}
							}
							mainImage.Source = CopyPixels(image, imageR, image.PixelWidth * 4, 0);
						}
						else if(!locked)
						{
							mainImage.Source = image;
						}
					}
				}
			}
		}

		public PixelData[,] GetPixels(BitmapSource source)
		{
			if(source.Format != PixelFormats.Bgra32)
				source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

			int width = source.PixelWidth;
			int height = source.PixelHeight;
			byte[] result = new byte[width * height * 4];

			var pixdata = new PixelData[width, height];

			source.CopyPixels(result, width * 4, 0);

			for(int i = 0; i<width; i++)
			{
				for(int j = 0; j < height; j++)
				{
					pixdata[i, j]       = new PixelData();
					pixdata[i, j].Blue  = result[4 * (i + j * width) + 0];
					pixdata[i, j].Green = result[4 * (i + j * width) + 1];
					pixdata[i, j].Red   = result[4 * (i + j * width) + 2];
					pixdata[i, j].Alpha = result[4 * (i + j * width) + 3];
				}
			}

			return pixdata;
		}

		public static BitmapSource CopyPixels(BitmapSource source, PixelData[,] pixels, int stride, int offset)
		{
			var height = source.PixelHeight;
			var width = source.PixelWidth;
			var pixelBytes = new byte[height * width * 4];
			source.CopyPixels(pixelBytes, 4 * width, 0);
			int y0 = offset / width;
			int x0 = offset - width * y0;
			for(int y = 0; y < height; y++)
				for(int x = 0; x < width; x++)
				{
					var pixel = pixels[x, y];
					int point = (y * width + x) * 4;
					pixelBytes[point + 0] = pixel.Blue;
					pixelBytes[point + 1] = pixel.Green;
					pixelBytes[point + 2] = pixel.Red;
					pixelBytes[point + 3] = pixel.Alpha;
				}

			return BitmapSource.Create(
				width,
				height,
				source.DpiX,
				source.DpiY,
				source.Format,
				new BitmapPalette(source,255),
				pixelBytes,
				width * 4
				);
			/*var encoder = new PngBitmapEncoder();
			var outputFrame = BitmapFrame.Create(newSource);
			encoder.Frames.Add(outputFrame);
			FileStream stream = new FileStream("test.png", FileMode.Create);
			encoder.Save(stream);*/
		}

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
