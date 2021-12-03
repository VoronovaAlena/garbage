using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Garbage
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private LibVLC _libvlc;
		private string video_url = $@"D:\Videos\2021-01-10-13.30.00.876-+0300.mkv";

		public MainWindow()
		{
			InitializeComponent();

			// Initialize VLC
			Core.Initialize();
			_libvlc = new LibVLC();
			vlcView.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libvlc);
			// video_URL is a video play address
			var m = new Media(_libvlc, video_url, FromType.FromLocation);
			m.AddOption(":rtsp-tcp");
			vlcView.MediaPlayer.Play(m);
		}
	}
}
