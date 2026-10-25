using System.Web.Http;

namespace System.Net.Http.Formatting.Parsers;

internal class HttpRequestHeaderParser
{
	private enum HttpRequestState
	{
		RequestLine,
		RequestHeaders
	}

	internal const int DefaultMaxRequestLineSize = 2048;

	internal const int DefaultMaxHeaderSize = 16384;

	private HttpUnsortedRequest _httpRequest;

	private HttpRequestState _requestStatus;

	private HttpRequestLineParser _requestLineParser;

	private InternetMessageFormatHeaderParser _headerParser;

	public HttpRequestHeaderParser(HttpUnsortedRequest httpRequest)
		: this(httpRequest, 2048, 16384)
	{
	}

	public HttpRequestHeaderParser(HttpUnsortedRequest httpRequest, int maxRequestLineSize, int maxHeaderSize)
	{
		if (httpRequest == null)
		{
			throw Error.ArgumentNull("httpRequest");
		}
		_httpRequest = httpRequest;
		_requestLineParser = new HttpRequestLineParser(_httpRequest, maxRequestLineSize);
		_headerParser = new InternetMessageFormatHeaderParser(_httpRequest.HttpHeaders, maxHeaderSize);
	}

	public ParserState ParseBuffer(byte[] buffer, int bytesReady, ref int bytesConsumed)
	{
		if (buffer == null)
		{
			throw Error.ArgumentNull("buffer");
		}
		ParserState result = ParserState.NeedMoreData;
		ParserState parserState = ParserState.NeedMoreData;
		HttpRequestState requestStatus = _requestStatus;
		if (requestStatus != 0)
		{
			if (requestStatus == HttpRequestState.RequestHeaders)
			{
				goto IL_004f;
			}
		}
		else
		{
			try
			{
				parserState = _requestLineParser.ParseBuffer(buffer, bytesReady, ref bytesConsumed);
			}
			catch (Exception)
			{
				parserState = ParserState.Invalid;
			}
			if (parserState == ParserState.Done)
			{
				_requestStatus = HttpRequestState.RequestHeaders;
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
