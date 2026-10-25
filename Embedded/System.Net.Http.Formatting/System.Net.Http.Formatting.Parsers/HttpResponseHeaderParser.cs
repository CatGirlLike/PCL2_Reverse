using System.Web.Http;

namespace System.Net.Http.Formatting.Parsers;

internal class HttpResponseHeaderParser
{
	private enum HttpResponseState
	{
		StatusLine,
		ResponseHeaders
	}

	internal const int DefaultMaxStatusLineSize = 2048;

	internal const int DefaultMaxHeaderSize = 16384;

	private HttpUnsortedResponse _httpResponse;

	private HttpResponseState _responseStatus;

	private HttpStatusLineParser _statusLineParser;

	private InternetMessageFormatHeaderParser _headerParser;

	public HttpResponseHeaderParser(HttpUnsortedResponse httpResponse)
		: this(httpResponse, 2048, 16384)
	{
	}

	public HttpResponseHeaderParser(HttpUnsortedResponse httpResponse, int maxResponseLineSize, int maxHeaderSize)
	{
		if (httpResponse == null)
		{
			throw Error.ArgumentNull("httpResponse");
		}
		_httpResponse = httpResponse;
		_statusLineParser = new HttpStatusLineParser(_httpResponse, maxResponseLineSize);
		_headerParser = new InternetMessageFormatHeaderParser(_httpResponse.HttpHeaders, maxHeaderSize);
	}

	public ParserState ParseBuffer(byte[] buffer, int bytesReady, ref int bytesConsumed)
	{
		if (buffer == null)
		{
			throw Error.ArgumentNull("buffer");
		}
		ParserState result = ParserState.NeedMoreData;
		ParserState parserState = ParserState.NeedMoreData;
		HttpResponseState responseStatus = _responseStatus;
		if (responseStatus != 0)
		{
			if (responseStatus == HttpResponseState.ResponseHeaders)
			{
				goto IL_004f;
			}
		}
		else
		{
			try
			{
				parserState = _statusLineParser.ParseBuffer(buffer, bytesReady, ref bytesConsumed);
			}
			catch (Exception)
			{
				parserState = ParserState.Invalid;
			}
			if (parserState == ParserState.Done)
			{
				_responseStatus = HttpResponseState.ResponseHeaders;
				parserState = ParserState.NeedMoreData;
				goto IL_004f;
			}
			if (parserState != 0)
			{
				result = parserState;
			}
		}
		goto IL_0078;
		IL_0078:
		return result;
		IL_004f:
		if (bytesConsumed < bytesReady)
		{
			try
			{
				parserState = _headerParser.ParseBuffer(buffer, bytesReady, ref bytesConsumed);
			}
			catch (Exception)
			{
				parserState = ParserState.Invalid;
			}
			switch (parserState)
			{
			case ParserState.Done:
				result = parserState;
				break;
			default:
				result = parserState;
				break;
			case ParserState.NeedMoreData:
				break;
			}
		}
		goto IL_0078;
	}
}
