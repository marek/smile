/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- Screenshot and Statistics Utility
// Copyright (c) 2005 Marek Kudlacz
//
// http://kudlacz.com
//
/////////////////////////////////////////////////////////////////////////////


using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace smiletray
{
	/// <summary>
	/// Summary description for NativeFunctions.
	/// </summary>
	public class NativeMethods
	{

		#region Dll Imports

		[DllImport("user32.dll", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=false)]
		public static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=false)]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=false)]
		public static extern IntPtr GetWindowDC(IntPtr hwnd);

		[DllImport("user32.dll", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=false)]
		public static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

		[DllImport("gdi32.dll", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=false)]
		public static extern UInt64 BitBlt
			(IntPtr hDestDC, int x, int y, int nWidth, int nHeight,
			IntPtr hSrcDC, int xSrc, int ySrc, Int32 dwRop);
		
		[DllImport("user32.dll",EntryPoint="FindWindow",CharSet=CharSet.Unicode)]
		public static extern int FindWindow(string ClassName, string WindowName);

		[DllImport("user32.dll")]
		public static extern IntPtr GetWindowRect(IntPtr hWnd,ref RECT rect);

		[DllImport("USER32.DLL", EntryPoint="BroadcastSystemMessageA",  SetLastError=true,
			 CharSet=CharSet.Unicode, ExactSpelling=true,
			 CallingConvention=CallingConvention.StdCall)]
		public static extern int BroadcastSystemMessage(Int32 dwFlags, ref Int32 pdwRecipients, int uiMessage, int wParam, int lParam);

		[DllImport("USER32.DLL", EntryPoint="RegisterWindowMessageA",  SetLastError=true,
			 CharSet=CharSet.Unicode, ExactSpelling=true,
			 CallingConvention=CallingConvention.StdCall)]
		public static extern int RegisterWindowMessage(String pString);
		
		#endregion

		public const Byte BSF_IGNORECURRENTTASK = 2; //this ignores the current app Hex 2
		public const Byte BSF_POSTMESSAGE = 16;  //This posts the message Hex 10
		public const Byte BSM_APPLICATIONS = 8;  //This tells the windows message to just go to applications Hex 8

		[StructLayout(LayoutKind.Sequential)]
			public struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
			public int width
			{
				get
				{
					return right - left;
				}
			}
			public int height
			{
				get
				{
					return bottom - top;
				}
			}
		}

	}


}
