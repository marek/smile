/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- Screenshot and Statistics Utility
// Copyright (c) 2005-2015 Marek Kudlacz
//
// http://kudlacz.com
//
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace smiletray
{

	public sealed class ScreenCapture
	{
		private const int SRCCOPY = 0x00CC0020;
		private const int CAPTUREBLT = 0x40000000;


		public static Image GetDesktopImage()
		{
			return GetDesktopImage(false);
		}
		// Get a screenshot of the current desktop
		public static Image GetDesktopImage(bool fullscreen)
		{
			Graphics desktopGraphics;
			Graphics desktopWindowGraphics;
			Image desktopImage;
			Rectangle screen = new Rectangle();

			IntPtr pDesktopWindow;
			if(fullscreen) 
			{
				pDesktopWindow = NativeMethods.GetDesktopWindow();
				screen = SystemInformation.VirtualScreen;
			}
			else
			{
				pDesktopWindow = NativeMethods.GetForegroundWindow();
				NativeMethods.RECT rect = new NativeMethods.RECT();
				NativeMethods.GetWindowRect(pDesktopWindow, ref rect);
				screen.X = screen.Y = 0;
				screen.Width = rect.width;
				screen.Height = rect.height;
			}

			desktopImage = new Bitmap(screen.Width, screen.Height);
			using (desktopGraphics = Graphics.FromImage(desktopImage))
			{
				IntPtr pDesktop = desktopGraphics.GetHdc();
				IntPtr pWindowDC = NativeMethods.GetWindowDC(pDesktopWindow);
				desktopWindowGraphics = Graphics.FromHdc(pWindowDC);


				desktopWindowGraphics.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);
				NativeMethods.BitBlt(pDesktop, 0, 0, screen.Width, screen.Height, pWindowDC, screen.X, screen.Y, SRCCOPY|CAPTUREBLT);

				// Release device contexts
				NativeMethods.ReleaseDC(pDesktopWindow, pWindowDC);
				desktopGraphics.ReleaseHdc(pDesktop);

				// Set pointers to zero.
				desktopWindowGraphics.Dispose();
				pDesktop = IntPtr.Zero;
				pDesktopWindow = IntPtr.Zero;
				pWindowDC = IntPtr.Zero;
			}
			desktopGraphics.Dispose();
			desktopWindowGraphics.Dispose();
			return desktopImage;
		}

		public static ImageCodecInfo GetEncoderInfo(String mimeType)
		{
			int j;
			ImageCodecInfo[] encoders;
			encoders = ImageCodecInfo.GetImageEncoders();
			for(j = 0; j < encoders.Length; ++j)
			{
				if(encoders[j].MimeType == mimeType)
					return encoders[j];
			}
			return null;
		}
	}
}