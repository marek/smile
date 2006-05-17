/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- CounterStrike: Source Screenshot and Statistics Utility
// v1.2
// Written by Marek Kudlacz
// Copyright (c)2005
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


		public static Image GetDesktopImage()
		{
			return GetDesktopImage(false);
		}
		// Get a screenshot of the current desktop
		public static Image GetDesktopImage(bool primary)
		{
			Graphics desktopGraphics;
			Graphics desktopWindowGraphics;
			Image desktopImage;
			Rectangle screen;
			if(primary) screen = Screen.PrimaryScreen.Bounds; 
			else screen = SystemInformation.VirtualScreen;
			desktopImage = new Bitmap(screen.Width, screen.Height);
			using (desktopGraphics = Graphics.FromImage(desktopImage))
			{
				IntPtr pDesktop = desktopGraphics.GetHdc();
				IntPtr pDesktopWindow = NativeMethods.GetDesktopWindow();
				IntPtr pWindowDC = NativeMethods.GetWindowDC(pDesktopWindow);
				desktopWindowGraphics = Graphics.FromHdc(pWindowDC);

				desktopWindowGraphics.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);
				NativeMethods.BitBlt(pDesktop, 0, 0, screen.Width, screen.Height, pWindowDC, screen.X, screen.Y, SRCCOPY);

				//Release device contexts
				NativeMethods.ReleaseDC(pDesktopWindow, pWindowDC);
				desktopGraphics.ReleaseHdc(pDesktop);

				//Set pointers to zero.
				desktopWindowGraphics.Dispose();
				pDesktop = IntPtr.Zero;
				pDesktopWindow = IntPtr.Zero;
				pWindowDC = IntPtr.Zero;
			}
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