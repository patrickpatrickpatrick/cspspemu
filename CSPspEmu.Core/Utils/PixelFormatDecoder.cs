﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Utils
{
	unsafe sealed public class PixelFormatDecoder
	{
		internal PixelFormatDecoder()
		{
		}

		public struct OutputPixel
		{
			public byte R, G, B, A;
			//public byte B, G, R, A;

			public override string ToString()
			{
				return String.Format("RGBA({0},{1},{2},{3})", R, G, B, A);
			}

			public int CheckSum { get { return (int)R + (int)G + (int)B + (int)A; } }
		}

		public struct Dxt1Block { }
		public struct Dxt3Block { }
		public struct Dxt5Block { }

		static readonly double[] Sizes =
		{
			// Rgba
			2, 2, 2, 4,
			// Palette
			0.5f, 1, 2, 4,
			// Compressed
			1, 1, 1
		};

		static public int GetPixelsSize(GuPixelFormats PixelFormat, int PixelCount)
		{
			return (int)(Sizes[(int)PixelFormat] * PixelCount);
		}

		void* _Input;
		byte* InputByte;
		ushort* InputShort;
		uint* InputInt;
		OutputPixel* Output;
		int Width;
		int Height;
		void* Palette;
		GuPixelFormats PaletteType;
		int PaletteCount;
		int PaletteStart;
		int PaletteShift;
		int PaletteMask;
		int StrideWidth;

		static public void Decode(GuPixelFormats PixelFormat, void* Input, OutputPixel* Output, int Width, int Height, void* Palette = null, GuPixelFormats PaletteType = GuPixelFormats.NONE, int PaletteCount = 0, int PaletteStart = 0, int PaletteShift = 0, int PaletteMask = 0xFF, int StrideWidth = -1, bool IgnoreAlpha = false)
		{
			if (StrideWidth == -1) StrideWidth = GetPixelsSize(PixelFormat, Width);
			var PixelFormatInt = (int)PixelFormat;
			var PixelFormatDecoder = new PixelFormatDecoder()
			{
				_Input = Input,
				InputByte = (byte *)Input,
				InputShort = (ushort*)Input,
				InputInt = (uint*)Input,
				Output = Output,
				StrideWidth = StrideWidth,
				Width = Width,
				Height = Height,
				Palette = Palette,
				PaletteType = PaletteType,
				PaletteCount = PaletteCount,
				PaletteStart = PaletteStart,
				PaletteShift = PaletteShift,
				PaletteMask = PaletteMask,
			};
			//Console.WriteLine(PixelFormat);
			switch (PixelFormat)
			{
				case GuPixelFormats.RGBA_5650: PixelFormatDecoder.Decode_RGBA_5650(); break;
				case GuPixelFormats.RGBA_5551: PixelFormatDecoder.Decode_RGBA_5551(); break;
				case GuPixelFormats.RGBA_4444: PixelFormatDecoder.Decode_RGBA_4444(); break;
				case GuPixelFormats.RGBA_8888: PixelFormatDecoder.Decode_RGBA_8888(); break;
				case GuPixelFormats.PALETTE_T4: PixelFormatDecoder.Decode_PALETTE_T4(); break;
				case GuPixelFormats.PALETTE_T8: PixelFormatDecoder.Decode_PALETTE_T8(); break;
				case GuPixelFormats.PALETTE_T16: PixelFormatDecoder.Decode_PALETTE_T16(); break;
				case GuPixelFormats.PALETTE_T32: PixelFormatDecoder.Decode_PALETTE_T32(); break;
				case GuPixelFormats.COMPRESSED_DXT1: PixelFormatDecoder.Decode_COMPRESSED_DXT1(); break;
				case GuPixelFormats.COMPRESSED_DXT3: PixelFormatDecoder.Decode_COMPRESSED_DXT3(); break;
				case GuPixelFormats.COMPRESSED_DXT5: PixelFormatDecoder.Decode_COMPRESSED_DXT5(); break;
				default: throw(new InvalidOperationException());
			}
			if (IgnoreAlpha)
			{
				for (int y = 0, n = 0; y < Height; y++) for (int x = 0; x < Width; x++, n++) Output[n].A = 0xFF;
			}
			//DecoderCallbackTable[PixelFormatInt](Input, Output, PixelCount, Width, Palette, PaletteType, PaletteCount, PaletteStart, PaletteShift, PaletteMask);
		}

		private unsafe void Decode_COMPRESSED_DXT5()
		{
			throw new NotImplementedException();
		}

		private unsafe void Decode_COMPRESSED_DXT3()
		{
			throw new NotImplementedException();
		}

		private unsafe void Decode_COMPRESSED_DXT1()
		{
			throw new NotImplementedException();
		}

		private unsafe void Decode_PALETTE_T32()
		{
			throw new NotImplementedException();
		}

		private unsafe void Decode_PALETTE_T16()
		{
			throw new NotImplementedException();
		}

		private unsafe void Decode_PALETTE_T8()
		{
			var Input = (byte*)_Input;

			if (Palette == null || PaletteType == GuPixelFormats.NONE) throw (new Exception("Palette required!"));
			OutputPixel[] PalettePixels;
			int PaletteSize = 256;
			PalettePixels = new OutputPixel[PaletteSize];
			var Translate = new int[PaletteSize];
			fixed (OutputPixel* PalettePixelsPtr = PalettePixels)
			{
				Decode(PaletteType, Palette, PalettePixelsPtr, PalettePixels.Length, 1);
				//Decode(PaletteType, Palette, PalettePixelsPtr, PaletteCount);
			}
			for (int n = 0; n < PaletteSize; n++)
			{
				Translate[n] = ((PaletteStart + n) >> PaletteShift) & PaletteMask;
			}

			for (int y = 0, n = 0; y < Height; y++)
			{
				var InputRow = (byte *)&InputByte[y * StrideWidth];
				for (int x = 0; x < Width; x++, n++)
				{
					byte Value = InputRow[x];
					Output[n] = PalettePixels[Translate[(Value >> 0) & 0xFF]];
				}
			}
		}

		private unsafe void Decode_PALETTE_T4()
		{
			var Input = (byte*)_Input;

			if (Palette == null || PaletteType == GuPixelFormats.NONE) throw(new Exception("Palette required!"));
			OutputPixel[] PalettePixels;
			int PaletteSize = 256;
			PalettePixels = new OutputPixel[PaletteSize];
			var Translate = new int[PaletteSize];
			fixed (OutputPixel* PalettePixelsPtr = PalettePixels)
			{
				Decode(PaletteType, Palette, PalettePixelsPtr, PalettePixels.Length, 1);
				//Decode(PaletteType, Palette, PalettePixelsPtr, PaletteCount);
			}
			//Console.WriteLine(PalettePixels.Length);
			for (int n = 0; n < 16; n++)
			{
				Translate[n] = ((PaletteStart + n) >> PaletteShift) & PaletteMask;
				//Console.WriteLine(PalettePixels[Translate[n]]);
			}

			for (int y = 0, n = 0; y < Height; y++)
			{
				var InputRow = (byte*)&InputByte[y * StrideWidth];
				for (int x = 0; x < Width / 2; x++, n++)
				{
					byte Value = InputRow[x];
					Output[n * 2 + 0] = PalettePixels[Translate[(Value >> 0) & 0xF]];
					Output[n * 2 + 1] = PalettePixels[Translate[(Value >> 4) & 0xF]];
				}
			}
		}

		private unsafe void Decode_RGBA_8888()
		{
			var Input = (uint*)_Input;

			for (int y = 0, n = 0; y < Height; y++)
			{
				var InputRow = (uint*)&InputByte[y * StrideWidth];
				for (int x = 0; x < Width; x++, n++)
				{
					OutputPixel Value = *((OutputPixel*)&InputRow[x]);
					Output[n].R = Value.R;
					Output[n].G = Value.G;
					Output[n].B = Value.B;
					Output[n].A = Value.A;
				}
			}
		}

		private unsafe void Decode_RGBA_4444()
		{
			//throw(new NotImplementedException());
			var Input = (ushort*)_Input;

			for (int y = 0, n = 0; y < Height; y++)
			{
				var InputRow = (ushort*)&InputByte[y * StrideWidth];
				for (int x = 0; x < Width; x++, n++)
				{
					Output[n] = Decode_RGBA_4444_Pixel(InputRow[x]);
				}
			}

		}

		private unsafe void Decode_RGBA_5551()
		{
			var Input = (ushort*)_Input;

			//long CheckSum = 0;

			for (int y = 0, n = 0; y < Height; y++)
			{
				var InputRow = (ushort*)&InputByte[y * StrideWidth];
				for (int x = 0; x < Width; x++, n++)
				{
					Decode_RGBA_5551_Pixel(InputRow[x], out Output[n]);
					//CheckSum += (long)Output[n].CheckSum;
				}
			}

			/*
			try
			{
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
				Console.Error.WriteLine("Decode_RGBA_5551 : {0}x{1} : {2}", Width, Height, CheckSum);
			}
			*/
		}

		private unsafe void Decode_RGBA_5650()
		{
			var Input = (ushort*)_Input;

			for (int y = 0, n = 0; y < Height; y++)
			{
				var InputRow = (ushort*)&InputByte[y * StrideWidth];
				for (int x = 0; x < Width; x++, n++)
				{
					Output[n] = Decode_RGBA_5650_Pixel(InputRow[x]);
				}
			}
		}

		static public unsafe OutputPixel Decode_RGBA_4444_Pixel(ushort Value)
		{
			return new OutputPixel()
			{
				R = (byte)Value.ExtractUnsignedScale(0, 4, 255),
				G = (byte)Value.ExtractUnsignedScale(4, 4, 255),
				B = (byte)Value.ExtractUnsignedScale(8, 4, 255),
				A = (byte)Value.ExtractUnsignedScale(12, 4, 255),
			};
		}

		static public unsafe void Decode_RGBA_5551_Pixel(ushort Value, out OutputPixel OutputPixel)
		{
#if true
			OutputPixel.R = (byte)(((Value >> 0) & 0x1F) * 255 / 0x1F);
			OutputPixel.G = (byte)(((Value >> 5) & 0x1F) * 255 / 0x1F);
			OutputPixel.B = (byte)(((Value >> 10) & 0x1F) * 255 / 0x1F);
			OutputPixel.A = (byte)(((Value >> 15) != 0) ? 255 : 0);
#else
			OutputPixel.R = (byte)Value.ExtractUnsignedScale(0, 5, 255);
			OutputPixel.G = (byte)Value.ExtractUnsignedScale(5, 5, 255);
			OutputPixel.B = (byte)Value.ExtractUnsignedScale(10, 5, 255);
			OutputPixel.A = (byte)Value.ExtractUnsignedScale(15, 1, 255);
#endif
		}

		static public unsafe OutputPixel Decode_RGBA_5650_Pixel(ushort Value)
		{
			return new OutputPixel()
			{
				R = (byte)Value.ExtractUnsignedScale(0, 5, 255),
				G = (byte)Value.ExtractUnsignedScale(5, 6, 255),
				B = (byte)Value.ExtractUnsignedScale(11, 5, 255),
				A = 0xFF,
			};
		}

		static public unsafe OutputPixel Decode_RGBA_8888_Pixel(uint Value)
		{
			return *(OutputPixel*)&Value;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Input"></param>
		/// <param name="Output"></param>
		/// <param name="RowWidth">Width of the texture. In bytes? In pixels? Maybe bytes?</param>
		/// <param name="TextureHeight">Height of the texture</param>
		static public unsafe void Unswizzle(byte[] Input, byte[] Output, int RowWidth, int TextureHeight)
		{
			fixed (void* InputPtr = Input)
			fixed (void* OutputPtr = Output)
			{
				Unswizzle(InputPtr, OutputPtr, RowWidth, TextureHeight);
			}
		}

		static public unsafe void Unswizzle(void* Input, void* Output, int RowWidth, int TextureHeight)
		{
			int pitch = (RowWidth - 16) / 4;
			int bxc = RowWidth / 16;
			int byc = TextureHeight / 8;

			var src = (uint*)Input;
			var ydest = (byte*)Output;
			for (int by = 0; by < byc; by++)
			{
				var xdest = ydest;
				for (int bx = 0; bx < bxc; bx++)
				{
					var dest = (uint*)xdest;
					for (int n = 0; n < 8; n++, dest += pitch)
					{
						*(dest++) = *(src++);
						*(dest++) = *(src++);
						*(dest++) = *(src++);
						*(dest++) = *(src++);
					}
					xdest += 16;
				}
				ydest += RowWidth * 8;
			}
		}

		static public unsafe void UnswizzleInline(void* Data, int RowWidth, int TextureHeight)
		{
			var Temp = new byte[RowWidth * TextureHeight];
			fixed (void* TempPointer = Temp)
			{
				Unswizzle(Data, TempPointer, RowWidth, TextureHeight);
			}
			Marshal.Copy(Temp, 0, new IntPtr(Data), RowWidth * TextureHeight);
		}

		public static unsafe void UnswizzleInline(GuPixelFormats Format, void* Data, int Width, int Height)
		{
			UnswizzleInline(Data, GetPixelsSize(Format, Width), Height);
		}

		public static unsafe uint Hash(GuPixelFormats PixelFormat, void* Input, int Width, int Height)
		{
			int TotalBytes = GetPixelsSize(PixelFormat, Width * Height);

			return Hashing.FastHash((uint*)Input, TotalBytes, (uint)((int)PixelFormat * Width * Height));
		}
	}
}
