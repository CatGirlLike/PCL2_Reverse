using System.Net.Http.Properties;
using System.Text;
using System.Web.Http;

namespace System.Net.Http.Formatting.Parsers;

internal class HttpRequestLineParser
{
	private enum HttpRequestLineState
	{
		RequestMethod,
		RequestUri,
		BeforeVersionNumbers,
		MajorVersionNumber,
		MinorVersionNumber,
		AfterCarriageReturn
	}

	internal const int MinRequestLineSize = 14;

	private const int DefaultTokenAllocation = 2048;

	private int _totalBytesConsumed;

	private int _maximumHeaderLength;

	private HttpRequestLineState _requestLineState;

	private HttpUnsortedRequest _httpRequest;

	private StringBuilder _currentToken = new StringBuilder(2048);

	public HttpRequestLineParser(HttpUnsortedRequest httpRequest, int maxRequestLineSize)
	{
		if (maxRequestLineSize < 14)
		{
			throw Error.ArgumentMustBeGreaterThanOrEqualTo("maxRequestLineSize", maxRequestLineSize, 14);
		}
		if (httpRequest == null)
		{
			throw Error.ArgumentNull("httpRequest");
		}
		_httpRequest = httpRequest;
		_maximumHeaderLength = maxRequestLineSize;
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
			return ParseRequestLine(buffer, bytesReady, ref bytesConsumed, ref _requestLineState, _maximumHeaderLength, ref _totalBytesConsumed, _currentToken, _httpRequest);
		}
		catch (Exception)
		{
			return ParserState.Invalid;
		}
	}

	private static ParserState ParseRequestLine(byte[] buffer, int bytesReady, ref int bytesConsumed, ref HttpRequestLineState requestLineState, int maximumHeaderLength, ref int totalBytesConsumed, StringBuilder currentToken, HttpUnsortedRequest httpRequest)
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
		string string5;
		string string6;
		string string8;
		string string10;
		switch (requestLineState)
		{
		case HttpRequestLineState.RequestMethod:
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 32)
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
			httpRequest.Method = new HttpMethod(currentToken.ToString());
			currentToken.Clear();
			requestLineState = HttpRequestLineState.RequestUri;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case HttpRequestLineState.RequestUri;
		case HttpRequestLineState.RequestUri:
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 32)
			{
				if (buffer[bytesConsumed] == 13)
				{
					goto IL_00fd;
				}
				if (++bytesConsumed != num2)
				{
					continue;
				}
				goto IL_0113;
			}
			if (bytesConsumed > num3)
			{
				string string4 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
				currentToken.Append(string4);
			}
			if (currentToken.Length == 0)
			{
				throw new FormatException(System.Net.Http.Properties.Resources.HttpMessageParserEmptyUri);
			}
			httpRequest.RequestUri = currentToken.ToString();
			currentToken.Clear();
			requestLineState = HttpRequestLineState.BeforeVersionNumbers;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case HttpRequestLineState.BeforeVersionNumbers;
		case HttpRequestLineState.BeforeVersionNumbers:
		{
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 47)
			{
				if (buffer[bytesConsumed] < 33 || buffer[bytesConsumed] > 122)
				{
					goto IL_01b1;
				}
				if (++bytesConsumed != num2)
				{
					continue;
				}
				goto IL_01c7;
			}
			if (bytesConsumed > num3)
			{
				string string9 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
				currentToken.Append(string9);
			}
			string text = currentToken.ToString();
			if (string.CompareOrdinal("HTTP", text) != 0)
			{
				throw new FormatException(Error.Format(System.Net.Http.Properties.Resources.HttpInvalidVersion, text, "HTTP"));
			}
			currentToken.Clear();
			requestLineState = HttpRequestLineState.MajorVersionNumber;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case HttpRequestLineState.MajorVersionNumber;
		}
		case HttpRequestLineState.MajorVersionNumber:
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 46)
			{
				if (buffer[bytesConsumed] < 48 || buffer[bytesConsumed] > 57)
				{
					goto IL_027d;
				}
				if (++bytesConsumed != num2)
				{
					continue;
				}
				goto IL_0293;
			}
			if (bytesConsumed > num3)
			{
				string string3 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
				currentToken.Append(string3);
			}
			currentToken.Append('.');
			requestLineState = HttpRequestLineState.MinorVersionNumber;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case HttpRequestLineState.MinorVersionNumber;
		case HttpRequestLineState.MinorVersionNumber:
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 13)
			{
				if (buffer[bytesConsumed] < 48 || buffer[bytesConsumed] > 57)
				{
					goto IL_0311;
				}
				if (++bytesConsumed != num2)
				{
					continue;
				}
				goto IL_0327;
			}
			if (bytesConsumed > num3)
			{
				string string7 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
				currentToken.Append(string7);
			}
			httpRequest.Version = Version.Parse(currentToken.ToString());
			currentToken.Clear();
			requestLineState = HttpRequestLineState.AfterCarriageReturn;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case HttpRequestLineState.AfterCarriageReturn;
		case HttpRequestLineState.AfterCarriageReturn:
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
			IL_00fd:
			result = ParserState.Invalid;
			break;
			IL_0076:
			string2 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
			currentToken.Append(string2);
			break;
			IL_0060:
			result = ParserState.Invalid;
			break;
			IL_0327:
			string5 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
			currentToken.Append(string5);
			break;
			IL_0311:
			result = ParserState.Invalid;
			break;
			IL_0293:
			string6 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
			currentToken.Append(string6);
			break;
			IL_027d:
			result = ParserState.Invalid;
			break;
			IL_01c7:
			string8 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
			currentToken.Append(string8);
			break;
			IL_01b1:
			result = ParserState.Invalid;
			break;
			IL_0113:
			string10 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
			currentToken.Append(string10);
			break;
		}
		totalBytesConsumed += bytesConsumed - num;
		return result;
	}
}
