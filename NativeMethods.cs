/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- CounterStrike: Source Screenshot and Statistics Utility
// v1.2
// Written by Marek Kudlacz
// Copyright (c)2005
//
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

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

		#endregion



	}
}
