/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- CounterStrike: Source Screenshot and Statistics Utility
// v1.2
// Written by Marek Kudlacz
// Copyright (c)2005
//
/////////////////////////////////////////////////////////////////////////////


using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Windows.Forms;
using Direct3D=Microsoft.DirectX.Direct3D;

namespace smiletray
{
	/// <summary>
	/// Summary description for DXScreenCapture.
	/// </summary>
	public class DXScreenCapture
	{
		public static Image GetDesktopImageD3D(String path)
		{
			try
			{
				Device device = null; // Our rendering device
				// Now  setup our D3D stuff
				// Now let's setup our D3D stuff
				PresentParameters presentParams = new PresentParameters();
				presentParams.Windowed=true;
				presentParams.SwapEffect = SwapEffect.Discard;
				device = new Device(0, DeviceType.Hardware, NativeMethods.GetForegroundWindow(), CreateFlags.SoftwareVertexProcessing, presentParams);
				Surface surface = device.CreateOffscreenPlainSurface(SystemInformation.VirtualScreen.Width,SystemInformation.VirtualScreen.Height,Direct3D.Format.A8R8G8B8, Direct3D.Pool.Scratch);
				device.GetFrontBufferData(0, surface);
				SurfaceLoader.Save(path, ImageFileFormat.Jpg, surface);
				surface.Dispose();
				device.Dispose();
			}
			catch ( Exception e ) 
			{
				Ex.DumpException(e);
			}
			return null;
		}

		public static Image GetDesktopImageDD()
		{
			
			return null;
		}
	}
}
