using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Garbage.Data
{
	[StructLayout(LayoutKind.Sequential)]
	public struct PixelData
	{
		public byte Blue;
		public byte Green;
		public byte Red;
		public byte Alpha;
	}
}
