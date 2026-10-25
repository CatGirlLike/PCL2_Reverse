using System.Text;

namespace System.Web.Http;

internal static class UriQueryUtility
{
	private class UrlDecoder
	{
		private int _bufferSize;

		private int _numChars;

		private char[] _charBuffer;

		private int _numBytes;

		private byte[] _byteBuffer;

		private Encoding _encoding;

		private void FlushBytes()
		{
			if (_numBytes > 0)
			{
				_numChars += _encoding.GetChars(_byteBuffer, 0, _numBytes, _charBuffer, _numChars);
				_numBytes = 0;
			}
		}

		internal UrlDecoder(int bufferSize, Encoding encoding)
		{
			_bufferSize = bufferSize;
			_encoding = encoding;
			_charBuffer = new char[bufferSize];
		}

		internal void AddChar(char ch)
		{
			if (_numBytes > 0)
			{
				FlushBytes();
			}
			_charBuffer[_numChars++] = ch;
		}

		internal void AddByte(byte b)
		{
			if (_byteBuffer == null)
			{
				_byteBuffer = new byte[_bufferSize];
			}
			_byteBuffer[_numBytes++] = b;
		}

		internal string GetString()
		{
			if (_numBytes > 0)
			{
				FlushBytes();
			}
			if (_numChars > 0)
			{
				return new string(_charBuffer, 0, _numChars);
			}
			return string.Empty;
		}
	}

	public static string UrlEncode(string str)
	{
		if (str == null)
		{
			return null;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(str);
		return Encoding.ASCII.GetString(UrlEncode(bytes, 0, bytes.Length, alwaysCreateNewReturnValue: false));
	}

	public static string UrlDecode(string str)
	{
		if (str == null)
		{
			return null;
		}
		return UrlDecodeInternal(str, Encoding.UTF8);
	}

	private static byte[] UrlEncode(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue)
	{
		byte[] array = UrlEncode(bytes, offset, count);
		if (!alwaysCreateNewReturnValue || array == null || array != bytes)
		{
			return array;
		}
		return (byte[])array.Clone();
	}

	private static byte[] UrlEncode(byte[] bytes, int offset, int count)
	{
		if (!ValidateUrlEncodingParameters(bytes, offset, count))
		{
			return null;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			char c = (char)bytes[offset + i];
			if (c == ' ')
			{
				num++;
			}
			else if (!IsUrlSafeChar(c))
			{
				num2++;
			}
		}
		if (num == 0 && num2 == 0)
		{
			return bytes;
		}
		byte[] array = new byte[count + num2 * 2];
		int num3 = 0;
		for (int j = 0; j < count; j++)
		{
			byte b = bytes[offset + j];
			char c2 = (char)b;
			if (IsUrlSafeChar(c2))
			{
				array[num3++] = b;
				continue;
			}
			if (c2 == ' ')
			{
				array[num3++] = 43;
				continue;
			}
			array[num3++] = 37;
			array[num3++] = (byte)IntToHex((b >> 4) & 0xF);
			array[num3++] = (byte)IntToHex(b & 0xF);
		}
		return array;
	}

	private static string UrlDecodeInternal(string value, Encoding encoding)
	{
		if (value == null)
		{
			return null;
		}
		int length = value.Length;
		UrlDecoder urlDecoder = new UrlDecoder(length, encoding);
		for (int i = 0; i < length; i++)
		{
			char c = value[i];
			switch (c)
			{
			case '+':
				c = ' ';
				break;
			case '%':
				if (i < length - 2)
				{
					int num = HexToInt(value[i + 1]);
					int num2 = HexToInt(value[i + 2]);
					if (num >= 0 && num2 >= 0)
					{
						byte b = (byte)((num << 4) | num2);
						i += 2;
						urlDecoder.AddByte(b);
						continue;
					}
				}
				break;
			}
			if ((c & 0xFF80) == 0)
			{
				urlDecoder.AddByte((byte)c);
			}
			else
			{
				urlDecoder.AddChar(c);
			}
		}
		return urlDecoder.GetString();
	}

	private static int HexToInt(char h)
	{
		if (h < '0' || h > '9')
		{
			if (h < 'a' || h > 'f')
			{
				if (h < 'A' || h > 'F')
				{
					return -1;
				}
				return h - 65 + 10;
			}
			return h - 97 + 10;
		}
		return h - 48;
	}

	private static char IntToHex(int n)
	{
		if (n <= 9)
		{
			return (char)(n + 48);
		}
		return (char)(n - 10 + 97);
	}

	private static bool IsUrlSafeChar(char ch)
	{
		if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
		{
			return true;
		}
		switch (ch)
		{
		case '!':
		case '(':
		case ')':
		case '*':
		case '-':
		case '.':
		case '_':
			return true;
		default:
			return false;
		}
	}

	private static bool ValidateUrlEncodingParameters(byte[] bytes, int offset, int count)
	{
		if (bytes == null && count == 0)
		{
			return false;
		}
		if (bytes == null)
		{
			throw Error.ArgumentNull("bytes");
		}
		if (offset < 0 || offset > bytes.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (count < 0 || offset + count > bytes.Length)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		return true;
	}
}
