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
using System.Threading;

namespace smiletray
{

	class CGifFile
	{
		public static void SaveAnimation(String file, ArrayList frames, int frameDelay)
		{
			SaveAnimation(file, frames, frameDelay, 0);
		}
		public static void SaveAnimation(String file, ArrayList frames, int frameDelay, int rest)
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

			Thread.Sleep(1);

			MemoryStream ms = new MemoryStream();
			BinaryWriter bw;
			Byte[] temp;
			// Create binary writer so we can write our bytes
			bw = new BinaryWriter(File.Open(file, FileMode.Create, FileAccess.Write, FileShare.None));
			Thread.Sleep(1);
			for(int i = 0; i < frames.Count; i++)
			{
				Image frame = (Image)frames[i];	
				frame.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);		// Save the image as a gif
				Thread.Sleep(1);
				temp = ms.ToArray();										// Copy the gif into a byte array
				Thread.Sleep(1);
				if(i == 0) 
				{
					bw.Write(temp, 0, 781);		// Copy Header & global color table from first frame
					Thread.Sleep(1);
					bw.Write(extblock, 0, 19);	// Copy the application extension we defined
					Thread.Sleep(1);
				}
				bw.Write(ctlblock, 0, 8);					// Graphic control block
				Thread.Sleep(1);
				bw.Write(temp, 789, temp.Length - 790);		// Image data
				Thread.Sleep(1);
				ms.SetLength(0);							// Flush current frame
				Thread.Sleep(rest);
			}
			bw.Write(";"); //Image terminator
			
			ms.Close();
			bw.Close();
			ms = null;
			bw = null;
			ctlblock = null;
			extblock = null;

		}

/*
		// Optimize Octree / Adaptive Color Table Creation Class
		private unsafe abstract class Quantizer
		{
			/// <summary>
			/// Construct the quantizer
			/// </summary>
			/// <param name="singlePass">If true, the quantization only needs to loop through the source pixels once</param>
			/// <remarks>
			/// If you construct this class with a true value for singlePass, then the code will, when quantizing your image,
			/// only call the 'QuantizeImage' function. If two passes are required, the code will call 'InitialQuantizeImage'
			/// and then 'QuantizeImage'.
			/// </remarks>
			public Quantizer ( bool singlePass )
			{
				_singlePass = singlePass ;
			}

			/// <summary>
			/// Quantize an image and return the resulting output bitmap
			/// </summary>
			/// <param name="source">The image to quantize</param>
			/// <returns>A quantized version of the image</returns>
			public Bitmap Quantize ( Image source )
			{
				// Get the size of the source image
				int	height = source.Height ;
				int width = source.Width ;

				// And construct a rectangle from these dimensions
				Rectangle	bounds = new Rectangle ( 0 , 0 , width , height ) ;

				// First off take a 32bpp copy of the image
				Bitmap	copy = new Bitmap ( width , height , PixelFormat.Format32bppArgb ) ;

				// And construct an 8bpp version
				Bitmap	output = new Bitmap ( width , height , PixelFormat.Format8bppIndexed ) ;

				// Now lock the bitmap into memory
				using ( Graphics g = Graphics.FromImage ( copy ) )
				{
					g.PageUnit = GraphicsUnit.Pixel ;

					// Draw the source image onto the copy bitmap,
					// which will effect a widening as appropriate.
					g.DrawImageUnscaled  ( source , bounds ) ;
				}

				// Define a pointer to the bitmap data
				BitmapData	sourceData = null ;

				try
				{
					// Get the source image bits and lock into memory
					sourceData = copy.LockBits ( bounds , ImageLockMode.ReadOnly , PixelFormat.Format32bppArgb ) ;

					// Call the FirstPass function if not a single pass algorithm.
					// For something like an octree quantizer, this will run through
					// all image pixels, build a data structure, and create a palette.
					if ( !_singlePass )
						FirstPass ( sourceData , width , height ) ;

					// Then set the color palette on the output bitmap. I'm passing in the current palette 
					// as there's no way to construct a new, empty palette.
					output.Palette = this.GetPalette ( output.Palette ) ;

					// Then call the second pass which actually does the conversion
					SecondPass ( sourceData , output , width , height , bounds ) ;
				}
				finally
				{
					// Ensure that the bits are unlocked
					copy.UnlockBits ( sourceData ) ;
				}

				// Last but not least, return the output bitmap
				return output;
			}

			/// <summary>
			/// Execute the first pass through the pixels in the image
			/// </summary>
			/// <param name="sourceData">The source data</param>
			/// <param name="width">The width in pixels of the image</param>
			/// <param name="height">The height in pixels of the image</param>
			protected virtual void FirstPass ( BitmapData sourceData , int width , int height )
			{
				// Define the source data pointers. The source row is a byte to
				// keep addition of the stride value easier (as this is in bytes)
				byte*	pSourceRow = (byte*)sourceData.Scan0.ToPointer ( ) ;
				Int32*	pSourcePixel ;

				// Loop through each row
				for ( int row = 0 ; row < height ; row++ )
				{
					// Set the source pixel to the first pixel in this row
					pSourcePixel = (Int32*) pSourceRow ;

					// And loop through each column
					for ( int col = 0 ; col < width ; col++ , pSourcePixel++ )
						// Now I have the pixel, call the FirstPassQuantize function...
						InitialQuantizePixel ( (Color32*)pSourcePixel ) ;

					// Add the stride to the source row
					pSourceRow += sourceData.Stride ;
				}
			}

			/// <summary>
			/// Execute a second pass through the bitmap
			/// </summary>
			/// <param name="sourceData">The source bitmap, locked into memory</param>
			/// <param name="output">The output bitmap</param>
			/// <param name="width">The width in pixels of the image</param>
			/// <param name="height">The height in pixels of the image</param>
			/// <param name="bounds">The bounding rectangle</param>
			protected virtual void SecondPass ( BitmapData sourceData , Bitmap output , int width , int height , Rectangle bounds )
			{
				BitmapData	outputData = null ;

				try
				{
					// Lock the output bitmap into memory
					outputData = output.LockBits ( bounds , ImageLockMode.WriteOnly , PixelFormat.Format8bppIndexed ) ;

					// Define the source data pointers. The source row is a byte to
					// keep addition of the stride value easier (as this is in bytes)
					byte*	pSourceRow = (byte*)sourceData.Scan0.ToPointer ( ) ;
					Int32*	pSourcePixel = (Int32*)pSourceRow ;
					Int32*	pPreviousPixel = pSourcePixel ;

					// Now define the destination data pointers
					byte*	pDestinationRow = (byte*) outputData.Scan0.ToPointer();
					byte*	pDestinationPixel = pDestinationRow ;

					// And convert the first pixel, so that I have values going into the loop
					byte	pixelValue = QuantizePixel ( (Color32*)pSourcePixel ) ;

					// Assign the value of the first pixel
					*pDestinationPixel = pixelValue ;

					// Loop through each row
					for ( int row = 0 ; row < height ; row++ )
					{
						// Set the source pixel to the first pixel in this row
						pSourcePixel = (Int32*) pSourceRow ;

						// And set the destination pixel pointer to the first pixel in the row
						pDestinationPixel = pDestinationRow ;

						// Loop through each pixel on this scan line
						for ( int col = 0 ; col < width ; col++ , pSourcePixel++ , pDestinationPixel++ )
						{
							// Check if this is the same as the last pixel. If so use that value
							// rather than calculating it again. This is an inexpensive optimisation.
							if ( *pPreviousPixel != *pSourcePixel )
							{
								// Quantize the pixel
								pixelValue = QuantizePixel ( (Color32*)pSourcePixel ) ;

								// And setup the previous pointer
								pPreviousPixel = pSourcePixel ;
							}

							// And set the pixel in the output
							*pDestinationPixel = pixelValue ;
						}

						// Add the stride to the source row
						pSourceRow += sourceData.Stride ;

						// And to the destination row
						pDestinationRow += outputData.Stride ;
					}
				}
				finally
				{
					// Ensure that I unlock the output bits
					output.UnlockBits ( outputData ) ;
				}
			}

			/// <summary>
			/// Override this to process the pixel in the first pass of the algorithm
			/// </summary>
			/// <param name="pixel">The pixel to quantize</param>
			/// <remarks>
			/// This function need only be overridden if your quantize algorithm needs two passes,
			/// such as an Octree quantizer.
			/// </remarks>
			protected virtual void InitialQuantizePixel ( Color32* pixel )
			{
			}

			/// <summary>
			/// Override this to process the pixel in the second pass of the algorithm
			/// </summary>
			/// <param name="pixel">The pixel to quantize</param>
			/// <returns>The quantized value</returns>
			protected abstract byte QuantizePixel ( Color32* pixel ) ;

			/// <summary>
			/// Retrieve the palette for the quantized image
			/// </summary>
			/// <param name="original">Any old palette, this is overrwritten</param>
			/// <returns>The new color palette</returns>
			protected abstract ColorPalette GetPalette ( ColorPalette original ) ;

			/// <summary>
			/// Flag used to indicate whether a single pass or two passes are needed for quantization.
			/// </summary>
			private bool	_singlePass ;

			/// <summary>
			/// Struct that defines a 32 bpp colour
			/// </summary>
			/// <remarks>
			/// This struct is used to read data from a 32 bits per pixel image
			/// in memory, and is ordered in this manner as this is the way that
			/// the data is layed out in memory
			/// </remarks>
			[StructLayout(LayoutKind.Explicit)]
				public struct Color32
			{
				[FieldOffset(0)]
				public byte Blue ;
				[FieldOffset(1)]
				public byte Green ;
				[FieldOffset(2)]
				public byte Red ;
				[FieldOffset(3)]
				public byte Alpha ;
				[FieldOffset(0)]
				public int ARGB ;
				public Color Color
				{
					get	{ return Color.FromArgb ( Alpha , Red , Green , Blue ) ; }
				}
			}
		}*/
	}

}
