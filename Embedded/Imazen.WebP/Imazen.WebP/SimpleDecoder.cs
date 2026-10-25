using System;
using System.Drawing;
using System.Drawing.Imaging;
using Imazen.WebP.Extern;

namespace Imazen.WebP;

public class SimpleDecoder
{
	public static string GetDecoderVersion()
	{
		int num = NativeMethods.WebPGetDecoderVersion();
		uint num2 = (uint)num % 256u;
		uint num3 = (uint)(num >>> 8) % 256u;
		uint num4 = (uint)(num >>> 16) % 256u;
		return num4 + "." + num3 + "." + num2;
	}

	public unsafe Bitmap DecodeFromBytes(byte[] data, long length)
	{
		fixed (byte* ptr = data)
		{
			return DecodeFromPointer((IntPtr)ptr, length);
		}
	}

	public Bitmap DecodeFromPointer(IntPtr data, long length)
	{
		int width = 0;
		int height = 0;
		if (NativeMethods.WebPGetInfo(data, (UIntPtr)(ulong)length, ref width, ref height) == 0)
		{
			throw new Exception("Invalid WebP header detected");
		}
		bool flag = false;
		Bitmap bitmap = null;
		BitmapData bitmapData = null;
		try
		{
			bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			IntPtr intPtr = NativeMethods.WebPDecodeBGRAInto(data, (UIntPtr)(ulong)length, bitmapData.Scan0, (UIntPtr)(ulong)(bitmapData.Stride * bitmapData.Height), bitmapData.Stride);
			if (bitmapData.Scan0 != intPtr)
			{
				throw new Exception("Failed to decode WebP image with error " + (long)intPtr);
			}
			flag = true;
		}
		finally
		{
			if (bitmapData != null)
			{
				bitmap?.UnlockBits(bitmapData);
			}
			if (!flag)
			{
				bitmap?.Dispose();
			}
		}
		return bitmap;
	}
}
