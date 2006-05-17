using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace smiletray
{

	public sealed class NativeMethods
	{
		private const int SRCCOPY = 0x00CC0020;

		#region Dll Imports

		[DllImport("user32.dll", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=false)]
		private static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=false)]
		private static extern IntPtr GetWindowDC(IntPtr hwnd);

		[DllImport("user32.dll", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=false)]
		private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

		[DllImport("gdi32.dll", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=false)]
		private static extern UInt64 BitBlt
			(IntPtr hDestDC, int x, int y, int nWidth, int nHeight,
			IntPtr hSrcDC, int xSrc, int ySrc, Int32 dwRop);

		#endregion

		// Get a screenshot of the current desktop
		public static Image GetDesktopBitmap()
		{
			Graphics desktopGraphics;
			Image desktopImage;
			Rectangle virtualScreen = SystemInformation.VirtualScreen;

			using (desktopImage = new Bitmap(virtualScreen.Width, virtualScreen.Height))
			{
				using (desktopGraphics = Graphics.FromImage(desktopImage))
				{
					IntPtr pDesktop = desktopGraphics.GetHdc();
					IntPtr pDesktopWindow = GetDesktopWindow();
					IntPtr pWindowDC = GetWindowDC(pDesktopWindow);

					BitBlt(pDesktop, 0, 0, virtualScreen.Width, virtualScreen.Height, pWindowDC, virtualScreen.X, virtualScreen.Y, SRCCOPY);

					//Release device contexts
					ReleaseDC(pDesktopWindow, pWindowDC);
					desktopGraphics.ReleaseHdc(pDesktop);

					//Set pointers to zero.
					pDesktop = IntPtr.Zero;
					pDesktopWindow = IntPtr.Zero;
					pWindowDC = IntPtr.Zero;
				}
			}

			return desktopImage;
		}
	}
}