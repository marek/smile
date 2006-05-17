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
using System.Drawing.Imaging;

namespace smiletray
{

	class CGifFile
	{
		public static void SaveAnimation(String file, ArrayList frames, int frameDelay)
		{
			SaveAnimation(file, frames, frameDelay, 0);
		}
		public static void SaveAnimation(String file, ArrayList frames, int frameDelay)
		{
			SaveAnimation(file, frames, frameDelay, 0, true);
		}
		public static void SaveAnimation(String file, ArrayList frames, int frameDelay, int rest, bool optimized)
		{
			if(frames.Count == 0)
				return;

			if(file == null || file.Trim().Length == 0)
				return;

			// Define the control block (such as frame delay)
			Byte[] ctlblock = new Byte[8];
			cb[0] = 33;									// Extension introducer
			cb[1] = 249;								// Graphic control extension
			cb[2] = 4;									// Size of block
			cb[3] = 1;									// [00000001] Flags:  reserved, no disposal method, no user input, transparent color
			cb[4] = (byte)(frameDelay & 0xFF);			// Delay time low byte
			cb[5] = (byte)((frameDelay >> 8) & 0xFF);	// Delay time high byte
			cb[6] = 255;								// Transparent color index
			cb[7] = 0;									// Block terminator

			// Image Descriptor
			Byte[] id = new Byte[10];
			id[0] = 44;			// Extension introducer
			id[1] = 0;			// Left position low byte
			id[2] = 0;			// Left position high byte
			id[3] = 0;			// Top position low byte
			id[4] = 0;			// Top position high byte
			id[5] = 0;			// Width low byte
			id[6] = 0;			// Width high byte
			id[7] = 0;			// Height low byte
			id[8] = 0;			// Height high byte
			id[9] = 135;		// [10000111] Flags: local color table, not interlaced, not sorted, reserved, 8bit= 256 colors = 2^((8 bit)-1)
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
				Image optimized_frame = Quantize(frame);
				Thread.Sleep(1);
				optimized_frame.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);		// Save the image as a gif
				frame.Dispose();
				frame = null;
				Thread.Sleep(1);
				temp = ms.ToArray();										// Copy the gif into a byte array
				Thread.Sleep(1);
				if(i == 0) 
				{
					// Define Header
					Byte[] header = new Byte[6];
					header[0] = (byte)'G';
					header[1] = (byte)'I';
					header[2] = (byte)'F';
					header[3] = (byte)'8';
					header[4] = (byte)'9';
					header[5] = (byte)'a';

					// Logical Screen Discriptor
					Byte[] lsd = new Byte[7];
					lsd[0] = (byte)(optimized_frame.Width & 0xFF);			// Width low byte
					lsd[1] = (byte)((optimized_frame.Width >> 8) & 0xFF);	// Width high byte
					lsd[2] = (byte)(optimized_frame.Height & 0xFF);			// Height low byte
					lsd[3] = (byte)((optimized_frame.Height >> 8) & 0xFF);	// Height high byte
					lsd[4] = 119;											// [01110111] Flags: No Global Color Table, 111=((8bit color)-1), Not ordered, 111 = n^((8 bits)-1) colors
					lsd[5] = 0;												// Background Color Index
					lsd[6] = 0;												// No pixel aspect ratio (it will determin itself)

					Byte[] extblock = new Byte[19];
					// Define the application extension (A netscape animation)
					ae[0] = 33;			// extension introducer
					ae[1] = 255;		// application extension
					ae[2] = 11;			// size of block
					ae[3] = (byte)'N';  // N		// App identifier
					ae[4] = (byte)'E';  // E
					ae[5] = (byte)'T';  // T
					ae[6] = (byte)'S';  // S
					ae[7] = (byte)'C';  // C
					ae[8] = (byte)'A';  // A
					ae[9] = (byte)'P';  // P
					ae[10] = (byte)'E'; // E
					ae[11] = (byte)'2'; // 2		// App Version
					ae[12] = (byte)'.'; // .
					ae[13] = (byte)'0'; // 0
					ae[14] = 3;			// Size of block
					ae[15] = 1;			//
					ae[16] = 0;			//
					ae[17] = 0;			//
					ae[18] = 0;			 // Block terminator

					// Fill in some of the Image Descriptor data here even though we dont write it
					id[5] = (byte)(optimized_frame.Width & 0xFF);			// Width low byte
					id[6] = (byte)((optimized_frame.Width >> 8) & 0xFF);	// Width high byte
					id[7] = (byte)(optimized_frame.Height & 0xFF);			// Height low byte
					id[8] = (byte)((optimized_frame.Height >> 8) & 0xFF);	// Height high byte

					bw.Write(header, 0, 6);		// Write Header
					Thread.Sleep(1);
					bw.Write(lsd, 0, 7);		// Write Logical Screen Discriptor
					Thread.Sleep(1);
					bw.Write(extblock, 0, 19);	// Copy the application extension we defined
					Thread.Sleep(1);
					lsd = null;
					extblock = null;

						
				}
				bw.Write(ctlblock, 0, 8);					// Graphic control block
				Thread.Sleep(1);
				bw.Write(id, 0, 10);						// Image Descriptor block
				Thread.Sleep(1);
				bw.Write(temp, 13, 768);					// Copy Optimized Color Table
				Thread.Sleep(1);							
				bw.Write(temp, 799, temp.Length - 800);		// Image data (skip over original header/screen descriptor/colortable/image descriptor)
				Thread.Sleep(1);
				ms.SetLength(0);							// Flush current frame
				Thread.Sleep(rest);
			}
			bw.Write(';'); //Image terminator
			
			ms.Close();
			bw.Close();
			ms = null;
			bw = null;
			ctlblock = null;
			

		}

		unsafe public static Bitmap Quantize ( Image source )
		{
			Octree octree = new Octree(8);

			// Get the size of the source image
			int	height = source.Height ;
			int width = source.Width ;

			// And construct a rectangle from these dimensions
			Rectangle	bounds = new Rectangle ( 0 , 0 , width , height ) ;

			// First off take a 32bpp copy of the image
			Bitmap	copy = new Bitmap ( width , height , System.Drawing.Imaging.PixelFormat.Format32bppArgb ) ;

			// And construct an 8bpp version
			Bitmap	output = new Bitmap ( width , height , System.Drawing.Imaging.PixelFormat.Format8bppIndexed ) ;

			// Now lock the bitmap into memory
			using ( Graphics g = Graphics.FromImage ( copy ) )
			{
				g.PageUnit = GraphicsUnit.Pixel ;

				// Draw the source image onto the copy bitmap,
				// which will effect a widening as appropriate.
				g.DrawImageUnscaled  ( source , bounds ) ;
			}

			// Define a pointer to the bitmap data
			System.Drawing.Imaging.BitmapData	sourceData = null ;

			try
			{
				// Get the source image bits and lock into memory
				sourceData = copy.LockBits ( bounds , System.Drawing.Imaging.ImageLockMode.ReadOnly , System.Drawing.Imaging.PixelFormat.Format32bppArgb ) ;

				// Call the FirstPass function if not a single pass algorithm.
				// For something like an octree quantizer, this will run through
				// all image pixels, build a data structure, and create a palette.
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
							octree.AddColor ( (Color32*)pSourcePixel ) ;

						// Add the stride to the source row
						pSourceRow += sourceData.Stride ;
					}
				}

				// Then set the color palette on the output bitmap. I'm passing in the current palette 
				// as there's no way to construct a new, empty palette.
				{
					System.Drawing.Imaging.ColorPalette c = output.Palette;
					// First off convert the octree to _maxColors colors
					ArrayList	palette = octree.Palletize ( 255 - 1 ) ;

					// Then convert the palette based on those colors
					for ( int index = 0 ; index < palette.Count ; index++ )
						c.Entries[index] = (Color)palette[index] ;

					// Add the transparent color
					c.Entries[255] = Color.FromArgb ( 0 , 0 , 0 , 0 ) ;
					output.Palette = c;
				}

				// Then call the second pass which actually does the conversion
				{
					System.Drawing.Imaging.BitmapData	outputData = null ;

					try
					{
						// Lock the output bitmap into memory
						outputData = output.LockBits ( bounds , System.Drawing.Imaging.ImageLockMode.WriteOnly , System.Drawing.Imaging.PixelFormat.Format8bppIndexed ) ;

						// Define the source data pointers. The source row is a byte to
						// keep addition of the stride value easier (as this is in bytes)
						byte*	pSourceRow = (byte*)sourceData.Scan0.ToPointer ( ) ;
						Int32*	pSourcePixel = (Int32*)pSourceRow ;
						Int32*	pPreviousPixel = pSourcePixel ;

						// Now define the destination data pointers
						byte*	pDestinationRow = (byte*) outputData.Scan0.ToPointer();
						byte*	pDestinationPixel = pDestinationRow ;

						// And convert the first pixel, so that I have values going into the loop
						// Get the palette index if this non-transparent
						byte	pixelValue = (byte)255;
						if ( ((Color32*)pSourcePixel)->Alpha > 0 )
							pixelValue = (byte)octree.GetPaletteIndex ((Color32*)pSourcePixel) ;

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
									pixelValue = (byte)200;
									if ( ((Color32*)pSourcePixel)->Alpha > 0 )
										pixelValue = (byte)octree.GetPaletteIndex ((Color32*)pSourcePixel) ;

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
			}
			finally
			{
				// Ensure that the bits are unlocked
				copy.UnlockBits ( sourceData ) ;
				copy.Dispose();
				copy = null;
			}

			// Last but not least, return the output bitmap
			return output;
		}

		unsafe public class Octree
		{
			// Construct the octree
			public Octree (int maxColorBits)
			{
				_maxColorBits = maxColorBits ;
				_leafCount = 0 ;
				_reducibleNodes = new OctreeNode[9] ;
				_root = new OctreeNode ( 0 , _maxColorBits , this ) ; 
				_previousColor = 0 ;
				_previousNode = null ;
			}

			// Add a given color value to the octree
			public void AddColor (Color32* pixel)
			{
				// Check if this request is for the same color as the last
				if ( _previousColor == pixel->ARGB )
				{
					// If so, check if I have a previous node setup. This will only ocurr if the first color in the image
					// happens to be black, with an alpha component of zero.
					if ( null == _previousNode )
					{
						_previousColor = pixel->ARGB ;
						_root.AddColor ( pixel , _maxColorBits , 0 , this ) ;
					}
					else
						// Just update the previous node
						_previousNode.Increment ( pixel ) ;
				}
				else
				{
					_previousColor = pixel->ARGB ;
					_root.AddColor ( pixel , _maxColorBits , 0 , this ) ;
				}
			}

			// Reduce the depth of the tree
			public void Reduce ( )
			{
				int	index ;

				// Find the deepest level containing at least one reducible node
				for ( index = _maxColorBits - 1 ; ( index > 0 ) && ( null == _reducibleNodes[index] ) ; index-- ) ;

				// Reduce the node most recently added to the list at level 'index'
				OctreeNode	node = _reducibleNodes[index] ;
				_reducibleNodes[index] = node.NextReducible ;

				// Decrement the leaf count after reducing the node
				_leafCount -= node.Reduce ( ) ;

				// And just in case I've reduced the last color to be added, and the next color to
				// be added is the same, invalidate the previousNode...
				_previousNode = null ;
			}

			// Get/Set the number of leaves in the tree
			public int Leaves
			{
				get { return _leafCount ; }
				set { _leafCount = value ; }
			}

			// Return the array of reducible nodes
			protected OctreeNode[] ReducibleNodes
			{
				get { return _reducibleNodes ; }
			}

			// Keep track of the previous node that was quantized
			protected void TrackPrevious ( OctreeNode node )
			{
				_previousNode = node ;
			}

			// Convert the nodes in the octree to a palette with a maximum of colorCount colors
			public ArrayList Palletize ( int colorCount )
			{
				while ( Leaves > colorCount )
					Reduce ( ) ;

				// Now palettize the nodes
				ArrayList	palette = new ArrayList ( Leaves ) ;
				int			paletteIndex = 0 ;
				_root.ConstructPalette ( palette , ref paletteIndex ) ;

				// And return the palette
				return palette ;
			}

			// Get the palette index for the passed color
			public int GetPaletteIndex ( Color32* pixel )
			{
				return _root.GetPaletteIndex ( pixel , 0 ) ;
			}

			// Mask used when getting the appropriate pixels for a given node
			private static int[] mask = new int[8] { 0x80 , 0x40 , 0x20 , 0x10 , 0x08 , 0x04 , 0x02 , 0x01 } ;

			// The root of the octree
			private	OctreeNode		_root ;

			// Number of leaves in the tree
			private int				_leafCount ;

			// Array of reducible nodes
			private OctreeNode[]	_reducibleNodes ;

			// Maximum number of significant bits in the image
			private int				_maxColorBits ;

			// Store the last node quantized
			private OctreeNode		_previousNode ;

			// Cache the previous color quantized
			private int				_previousColor ;

			// Class which encapsulates each node in the tree
			protected class OctreeNode
			{
				// Construct the node
				public OctreeNode ( int level , int colorBits , Octree octree )
				{
					// Construct the new node
					_leaf = ( level == colorBits ) ;

					_red = _green = _blue = 0 ;
					_pixelCount = 0 ;

					// If a leaf, increment the leaf count
					if ( _leaf )
					{
						octree.Leaves++ ;
						_nextReducible = null ;
						_children = null ; 
					}
					else
					{
						// Otherwise add this to the reducible nodes
						_nextReducible = octree.ReducibleNodes[level] ;
						octree.ReducibleNodes[level] = this ;
						_children = new OctreeNode[8] ;
					}
				}

				// Add a color into the tree
				public void AddColor ( Color32* pixel , int colorBits , int level , Octree octree )
				{
					// Update the color information if this is a leaf
					if ( _leaf )
					{
						Increment ( pixel ) ;
						// Setup the previous node
						octree.TrackPrevious ( this ) ;
					}
					else
					{
						// Go to the next level down in the tree
						int	shift = 7 - level ;
						int index = ( ( pixel->Red & mask[level] ) >> ( shift - 2 ) ) |
							( ( pixel->Green & mask[level] ) >> ( shift - 1 ) ) |
							( ( pixel->Blue & mask[level] ) >> ( shift ) ) ;

						OctreeNode	child = _children[index] ;

						if ( null == child )
						{
							// Create a new child node & store in the array
							child = new OctreeNode ( level + 1 , colorBits , octree ) ; 
							_children[index] = child ;
						}

						// Add the color to the child node
						child.AddColor ( pixel , colorBits , level + 1 , octree ) ;
					}

				}

				// Get/Set the next reducible node
				public OctreeNode NextReducible
				{
					get { return _nextReducible ; }
					set { _nextReducible = value ; }
				}

				// Return the child nodes
				public OctreeNode[] Children
				{
					get { return _children ; }
				}

				// Reduce this node by removing all of its children
				public int Reduce ( )
				{
					_red = _green = _blue = 0 ;
					int	children = 0 ;

					// Loop through all children and add their information to this node
					for ( int index = 0 ; index < 8 ; index++ )
					{
						if ( null != _children[index] )
						{
							_red += _children[index]._red ;
							_green += _children[index]._green ;
							_blue += _children[index]._blue ;
							_pixelCount += _children[index]._pixelCount ;
							++children ;
							_children[index] = null ;
						}
					}

					// Now change this to a leaf node
					_leaf = true ;

					// Return the number of nodes to decrement the leaf count by
					return ( children - 1 ) ;
				}

				// Traverse the tree, building up the color palette
				public void ConstructPalette ( ArrayList palette , ref int paletteIndex )
				{
					if ( _leaf )
					{
						// Consume the next palette index
						_paletteIndex = paletteIndex++ ;

						// And set the color of the palette entry
						palette.Add ( Color.FromArgb ( _red / _pixelCount , _green / _pixelCount , _blue / _pixelCount ) ) ;
					}
					else
					{
						// Loop through children looking for leaves
						for ( int index = 0 ; index < 8 ; index++ )
						{
							if ( null != _children[index] )
								_children[index].ConstructPalette ( palette , ref paletteIndex ) ;
						}
					}
				}

				// Return the palette index for the passed color
				public int GetPaletteIndex ( Color32* pixel , int level )
				{
					int	paletteIndex = _paletteIndex ;

					if ( !_leaf )
					{
						int	shift = 7 - level ;
						int index = ( ( pixel->Red & mask[level] ) >> ( shift - 2 ) ) |
							( ( pixel->Green & mask[level] ) >> ( shift - 1 ) ) |
							( ( pixel->Blue & mask[level] ) >> ( shift ) ) ;

						if ( null != _children[index] )
							paletteIndex = _children[index].GetPaletteIndex ( pixel , level + 1 ) ;
						else
							throw new Exception ( "Didn't expect this!" ) ;
					}

					return paletteIndex ;
				}

				// Increment the pixel count and add to the color information
				public void Increment ( Color32* pixel )
				{
					_pixelCount++ ;
					_red += pixel->Red ;
					_green += pixel->Green ;
					_blue += pixel->Blue ;
				}

				// Flag indicating that this is a leaf node
				private	bool			_leaf ;

				// Number of pixels in this node
				private	int				_pixelCount ;

				// Color components
				private	int				_red ;
				private	int				_green ;
				private int				_blue ;

				// Pointers to any child nodes
				private OctreeNode[]	_children ;

				// Pointer to next reducible node
				private OctreeNode		_nextReducible ;

				// The index of this node in the palette
				private	int				_paletteIndex ;

			}
		}
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
	}

}
