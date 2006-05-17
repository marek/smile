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

		private NativeMethods()
		{}

		public static Image GetDesktopBitmap()
		{
			Rectangle virtualScreen = SystemInformation.VirtualScreen;
			return GetDesktopBitmap(virtualScreen.X, virtualScreen.Y, virtualScreen.Width, virtualScreen.Height);
		}

		public static Image GetDesktopBitmap(Rectangle rectangle)
		{
			return GetDesktopBitmap(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		}

		public static Image GetDesktopBitmap(int x, int y, int width, int height)
		{
			Graphics desktopGraphics;
			Image desktopImage;
			Graphics drawGraphics;
			Image drawImage;
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

				drawImage = new Bitmap(width, height);
				using (drawGraphics = Graphics.FromImage(drawImage))
				{
					//Draw the area of the desktop we want into the new image.
					drawGraphics.DrawImage(
						desktopImage,
						new Rectangle(new Point(0, 0), new Size(width, height)),
						new Rectangle((virtualScreen.X - x)*-1, (virtualScreen.Y - y)*-1, width, height),
						GraphicsUnit.Pixel);
				}
			}

			return drawImage;
		}
	}
}