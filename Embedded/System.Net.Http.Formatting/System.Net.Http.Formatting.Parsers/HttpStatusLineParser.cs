using System.Globalization;
using System.Net.Http.Properties;
using System.Text;
using System.Web.Http;

namespace System.Net.Http.Formatting.Parsers;

internal class HttpStatusLineParser
{
	private enum HttpStatusLineState
	{
		BeforeVersionNumbers,
		MajorVersionNumber,
		MinorVersionNumber,
		StatusCode,
		ReasonPhrase,
		AfterCarriageReturn
	}

	internal const int MinStatusLineSize = 15;

	private const int DefaultTokenAllocation = 2048;

	private const int MaxStatusCode = 1000;

	private int _totalBytesConsumed;

	private int _maximumHeaderLength;

	private HttpStatusLineState _statusLineState;

	private HttpUnsortedResponse _httpResponse;

	private StringBuilder _currentToken = new StringBuilder(2048);

	public HttpStatusLineParser(HttpUnsortedResponse httpResponse, int maxStatusLineSize)
	{
		if (maxStatusLineSize < 15)
		{
			throw Error.ArgumentMustBeGreaterThanOrEqualTo("maxStatusLineSize", maxStatusLineSize, 15);
		}
		if (httpResponse == null)
		{
			throw Error.ArgumentNull("httpResponse");
		}
		_httpResponse = httpResponse;
		_maximumHeaderLength = maxStatusLineSize;
	}

	public ParserState ParseBuffer(byte[] buffer, int bytesReady, ref int bytesConsumed)
	{
		if (buffer == null)
		{
			throw Error.ArgumentNull("buffer");
		}
		ParserState result = ParserState.NeedMoreData;
		if (bytesConsumed >= bytesReady)
		{
			return result;
		}
		try
		{
			return ParseStatusLine(buffer, bytesReady, ref bytesConsumed, ref _statusLineState, _maximumHeaderLength, ref _totalBytesConsumed, _currentToken, _httpResponse);
		}
		catch (Exception)
		{
			return ParserState.Invalid;
		}
	}

	private static ParserState ParseStatusLine(byte[] buffer, int bytesReady, ref int bytesConsumed, ref HttpStatusLineState statusLineState, int maximumHeaderLength, ref int totalBytesConsumed, StringBuilder currentToken, HttpUnsortedResponse httpResponse)
	{
		int num = bytesConsumed;
		ParserState result = ParserState.DataTooBig;
		int num2 = ((maximumHeaderLength <= 0) ? int.MaxValue : (maximumHeaderLength - totalBytesConsumed + bytesConsumed));
		if (bytesReady < num2)
		{
			result = ParserState.NeedMoreData;
			num2 = bytesReady;
		}
		int num3;
		string string2;
		string string4;
		string string5;
		string string7;
		string string8;
		switch (statusLineState)
		{
		case HttpStatusLineState.BeforeVersionNumbers:
		{
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 47)
			{
				if (buffer[bytesConsumed] < 33 || buffer[bytesConsumed] > 122)
				{
					goto IL_0060;
				}
				if (++bytesConsumed != num2)
				{
					continue;
				}
				goto IL_0076;
			}
			if (bytesConsumed > num3)
			{
				string @string = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
				currentToken.Append(@string);
			}
			string text = currentToken.ToString();
			if (string.CompareOrdinal("HTTP", text) != 0)
			{
				throw new FormatException(Error.Format(System.Net.Http.Properties.Resources.HttpInvalidVersion, text, "HTTP"));
			}
			currentToken.Clear();
			statusLineState = HttpStatusLineState.MajorVersionNumber;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case HttpStatusLineState.MajorVersionNumber;
		}
		case HttpStatusLineState.MajorVersionNumber:
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 46)
			{
				if (buffer[bytesConsumed] < 48 || buffer[bytesConsumed] > 57)
				{
					goto IL_012c;
				}
				if (++bytesConsumed != num2)
				{
					continue;
				}
				goto IL_0142;
			}
			if (bytesConsumed > num3)
			{
				string string6 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
				currentToken.Append(string6);
			}
			currentToken.Append('.');
			statusLineState = HttpStatusLineState.MinorVersionNumber;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case HttpStatusLineState.MinorVersionNumber;
		case HttpStatusLineState.MinorVersionNumber:
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 32)
			{
				if (buffer[bytesConsumed] < 48 || buffer[bytesConsumed] > 57)
				{
					goto IL_01c0;
				}
				if (++bytesConsumed != num2)
				{
					continue;
				}
				goto IL_01d6;
			}
			if (bytesConsumed > num3)
			{
				string string10 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
				currentToken.Append(string10);
			}
			httpResponse.Version = Version.Parse(currentToken.ToString());
			currentToken.Clear();
			statusLineState = HttpStatusLineState.StatusCode;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case HttpStatusLineState.StatusCode;
		case HttpStatusLineState.StatusCode:
		{
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 32)
			{
				if (buffer[bytesConsumed] < 48 || buffer[bytesConsumed] > 57)
				{
					goto IL_0265;
				}
				if (++bytesConsumed != num2)
				{
					continue;
				}
				goto IL_027b;
			}
			if (bytesConsumed > num3)
			{
				string string3 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
				currentToken.Append(string3);
			}
			int num4 = int.Parse(currentToken.ToString(), CultureInfo.InvariantCulture);
			if (num4 < 100 || num4 > 1000)
			{
				throw new FormatException(Error.Format(System.Net.Http.Properties.Resources.HttpInvalidStatusCode, num4, 100, 1000));
			}
			httpResponse.StatusCode = (HttpStatusCode)num4;
			currentToken.Clear();
			statusLineState = HttpStatusLineState.ReasonPhrase;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case HttpStatusLineState.ReasonPhrase;
		}
		case HttpStatusLineState.ReasonPhrase:
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 13)
			{
				if (buffer[bytesConsumed] < 32 || buffer[bytesConsumed] > 122)
				{
					goto IL_0359;
				}
				if (++bytesConsumed != num2)
				{
					continue;
				}
				goto IL_036f;
			}
			if (bytesConsumed > num3)
			{
				string string9 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
				currentToken.Append(string9);
			}
			httpResponse.ReasonPhrase = currentToken.ToString();
			currentToken.Clear();
			statusLineState = HttpStatusLineState.AfterCarriageReturn;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case HttpStatusLineState.AfterCarriageReturn;
		case HttpStatusLineState.AfterCarriageReturn:
			{
				if (buffer[bytesConsumed] != 10)
				{
					result = ParserState.Invalid;
					break;
				}
				result = ParserState.Done;
				bytesConsumed++;
				break;
			}
			IL_0142:
			string2 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
			currentToken.Append(string2);
			break;
			IL_0060:
			result = ParserState.Invalid;
			break;
			IL_0076:
			string4 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
			currentToken.Append(string4);
			break;
			IL_0359:
			result = ParserState.Invalid;
			break;
			IL_036f:
			string5 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
			currentToken.Append(string5);
			break;
			IL_027b:
			string7 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
			currentToken.Append(string7);
			break;
			IL_0265:
			result = ParserState.Invalid;
			break;
			IL_01d6:
			string8 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
			currentToken.Append(string8);
			break;
			IL_01c0:
			result = ParserState.Invalid;
			break;
			IL_012c:
			result = ParserState.Invalid;
			break;
		}
		totalBytesConsumed += bytesConsumed - num;
		return result;
	}
}
