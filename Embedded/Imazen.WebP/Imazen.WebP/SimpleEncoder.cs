using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Imazen.WebP.Extern;

namespace Imazen.WebP;

public class SimpleEncoder
{
	public static string GetEncoderVersion()
	{
		int num = NativeMethods.WebPGetEncoderVersion();
		uint num2 = (uint)num % 256u;
		uint num3 = (uint)(num >>> 8) % 256u;
		uint num4 = (uint)(num >>> 16) % 256u;
		return num4 + "." + num3 + "." + num2;
	}

	[Obsolete]
	public void Encode(Bitmap from, Stream to, float quality, bool noAlpha)
	{
		Encode(from, to, quality);
	}

	public void Encode(Bitmap from, Stream to, float quality)
	{
		Encode(from, quality, out var result, out var length);
		try
		{
			byte[] array = new byte[4096];
			for (int i = 0; i < length; i += array.Length)
			{
				int num = (int)Math.Min(array.Length, length - i);
				Marshal.Copy((IntPtr)((long)result + i), array, 0, num);
				to.Write(array, 0, num);
			}
		}
		finally
		{
			NativeMethods.WebPSafeFree(result);
		}
	}

	[Obsolete]
	public void Encode(Bitmap b, float quality, bool noAlpha, out IntPtr result, out long length)
	{
		Encode(b, quality, out result, out length);
	}

	public void Encode(Bitmap b, float quality, out IntPtr result, out long length)
	{
		if (quality < -1f)
		{
			quality = -1f;
		}
		if (quality > 100f)
		{
			quality = 100f;
		}
		int width = b.Width;
		int height = b.Height;
		BitmapData bitmapData = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, b.PixelFormat);
		try
		{
			result = IntPtr.Zero;
			if (b.PixelFormat == PixelFormat.Format32bppArgb)
			{
				if (quality == -1f)
				{
					length = (long)(ulong)NativeMethods.WebPEncodeLosslessBGRA(bitmapData.Scan0, width, height, bitmapData.Stride, ref result);
				}
				else
				{
					length = (long)NativeMethods.WebPEncodeBGRA(bitmapData.Scan0, width, height, bitmapData.Stride, quality, ref result);
				}
			}
			else if (b.PixelFormat == PixelFormat.Format24bppRgb)
			{
				if (quality == -1f)
				{
					length = (long)(ulong)NativeMethods.WebPEncodeLosslessBGR(bitmapData.Scan0, width, height, bitmapData.Stride, ref result);
				}
				else
				{
					length = (long)(ulong)NativeMethods.WebPEncodeBGR(bitmapData.Scan0, width, height, bitmapData.Stride, quality, ref result);
				}
			}
			else
			{
				using Bitmap b2 = b.Clone(new Rectangle(0, 0, b.Width, b.Height), PixelFormat.Format32bppArgb);
				Encode(b2, quality, out result, out length);
			}
			if (length == 0L)
			{
				throw new Exception("WebP encode failed!");
			}
		}
		finally
		{
			b.UnlockBits(bitmapData);
		}
	}
}
