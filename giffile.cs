/* 
GIF89a Exporter

So, .NET does not support animated gifs, only single framed ones. So what must one do? Make it yourself

For the official specifications:
http://www.netsw.org/graphic/bitmap/formats/gif/gifmerge/docs/gifspecs.txt

In short it can be described with:
---
GIF89a HEADER 
LOGICAL SCREEN DESCRIPTOR BLOCK 
optional GLOBAL COLOR TABLE
optional NETSCAPE APPLICATION EXTENSION BLOCK(:-> surprise) 
a collection of graphic blocks 
GIF TRAILER ends the series of images 
--

What we need to define is the application extension, and graphic control blocks. The rest will be copied from
information we already have (the one framed gif we CAN make).

*/

using System;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

namespace smiletray
{

	class CGifFile
	{
		public static void SaveAnimation(String file, ArrayList frames, int frameDelay)
		{
			if(frames.Count == 0)
				return;

			if(file == null || file.Trim().Length == 0)
				return;

			Byte[] extblock = new Byte[19];
			// Define the application extension (A netscape animation)
			extblock[0] = 33;  // extension introducer
			extblock[1] = 255; // application extension
			extblock[2] = 11;  // size of block
			extblock[3] = 78;  // N		// App identifier
			extblock[4] = 69;  // E
			extblock[5] = 84;  // T
			extblock[6] = 83;  // S
			extblock[7] = 67;  // C
			extblock[8] = 65;  // A
			extblock[9] = 80;  // P
			extblock[10] = 69; // E
			extblock[11] = 50; // 2		// App Version
			extblock[12] = 46; // .
			extblock[13] = 48; // 0
			extblock[14] = 3;  // Size of block
			extblock[15] = 1;  //
			extblock[16] = 0;  //
			extblock[17] = 0;  //
			extblock[18] = 0;  // Block terminator

			// Define the control block (such as frame delay)
			Byte[] ctlblock = new Byte[8];
			ctlblock[0] = 33;  // Extension introducer
			ctlblock[1] = 249; // Graphic control extension
			ctlblock[2] = 4;   // Size of block
			ctlblock[3] = 9;   // Flags: reserved, disposal method, user input, transparent color
			ctlblock[4] = (byte)(frameDelay & 0xFF);		// Delay time low byte
			ctlblock[5] = (byte)((frameDelay >> 8) & 0xFF);	// Delay time high byte
			ctlblock[6] = 255; // Transparent color index
			ctlblock[7] = 0;   // Block terminator

			MemoryStream ms = new MemoryStream();
			BinaryWriter bw;
			Byte[] temp;
			// Create binary writer so we can write our bytes
			bw = new BinaryWriter(File.Open(file, FileMode.Create, FileAccess.Write, FileShare.None));
			for(int i = 0; i < frames.Count; i++)
			{
				Image frame = (Image)frames[i];	
				frame.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);		// Save the image as a gif
				temp = ms.ToArray();										// Copy the gif into a byte array
				if(i == 0) 
				{
					bw.Write(temp, 0, 781);		// Copy Header & global color table from first frame
					bw.Write(extblock, 0, 19);	// Copy the application extension we defined
				}
				bw.Write(ctlblock, 0, 8);					// Graphic control block
				bw.Write(temp, 789, temp.Length - 790);		// Image data
				ms.SetLength(0);							// Flush current frame
			}
			bw.Write(";"); //Image terminator
			ms.Close();
			bw.Close();
		}
	}

}
