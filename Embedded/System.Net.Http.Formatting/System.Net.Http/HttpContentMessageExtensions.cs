using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting.Parsers;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class HttpContentMessageExtensions
{
	private const int MinBufferSize = 256;

	private const int DefaultBufferSize = 32768;

	public static bool IsHttpRequestMessageContent(this HttpContent content)
	{
		if (content == null)
		{
			throw Error.ArgumentNull("content");
		}
		try
		{
			return HttpMessageContent.ValidateHttpMessageContent(content, isRequest: true, throwOnError: false);
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static bool IsHttpResponseMessageContent(this HttpContent content)
	{
		if (content == null)
		{
			throw Error.ArgumentNull("content");
		}
		try
		{
			return HttpMessageContent.ValidateHttpMessageContent(content, isRequest: false, throwOnError: false);
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content)
	{
		return content.ReadAsHttpRequestMessageAsync("http", 32768);
	}

	public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content, CancellationToken cancellationToken)
	{
		return content.ReadAsHttpRequestMessageAsync("http", 32768, cancellationToken);
	}

	public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content, string uriScheme)
	{
		return content.ReadAsHttpRequestMessageAsync(uriScheme, 32768);
	}

	public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content, string uriScheme, CancellationToken cancellationToken)
	{
		return content.ReadAsHttpRequestMessageAsync(uriScheme, 32768, cancellationToken);
	}

	public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content, string uriScheme, int bufferSize)
	{
		return content.ReadAsHttpRequestMessageAsync(uriScheme, bufferSize, 16384);
	}

	public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content, string uriScheme, int bufferSize, CancellationToken cancellationToken)
	{
		return content.ReadAsHttpRequestMessageAsync(uriScheme, bufferSize, 16384, cancellationToken);
	}

	public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content, string uriScheme, int bufferSize, int maxHeaderSize)
	{
		return content.ReadAsHttpRequestMessageAsync(uriScheme, bufferSize, maxHeaderSize, CancellationToken.None);
	}

	public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content, string uriScheme, int bufferSize, int maxHeaderSize, CancellationToken cancellationToken)
	{
		if (content == null)
		{
			throw Error.ArgumentNull("content");
		}
		if (uriScheme == null)
		{
			throw Error.ArgumentNull("uriScheme");
		}
		if (!Uri.CheckSchemeName(uriScheme))
		{
			throw Error.Argument("uriScheme", System.Net.Http.Properties.Resources.HttpMessageParserInvalidUriScheme, uriScheme, typeof(Uri).Name);
		}
		if (bufferSize < 256)
		{
			throw Error.ArgumentMustBeGreaterThanOrEqualTo("bufferSize", bufferSize, 256);
		}
		if (maxHeaderSize < 2)
		{
			throw Error.ArgumentMustBeGreaterThanOrEqualTo("maxHeaderSize", maxHeaderSize, 2);
		}
		HttpMessageContent.ValidateHttpMessageContent(content, isRequest: true, throwOnError: true);
		return content.ReadAsHttpRequestMessageAsyncCore(uriScheme, bufferSize, maxHeaderSize, cancellationToken);
	}

	private static async Task<HttpRequestMessage> ReadAsHttpRequestMessageAsyncCore(this HttpContent content, string uriScheme, int bufferSize, int maxHeaderSize, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		Stream stream = await content.ReadAsStreamAsync();
		HttpUnsortedRequest httpRequest = new HttpUnsortedRequest();
		HttpRequestHeaderParser parser = new HttpRequestHeaderParser(httpRequest, Math.Max(2048, maxHeaderSize), maxHeaderSize);
		byte[] buffer = new byte[bufferSize];
		int headerConsumed = 0;
		int num;
		do
		{
			try
			{
				num = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
			}
			catch (Exception innerException)
			{
				throw new IOException(System.Net.Http.Properties.Resources.HttpMessageErrorReading, innerException);
			}
			ParserState parserState;
			try
			{
				parserState = parser.ParseBuffer(buffer, num, ref headerConsumed);
			}
			catch (Exception)
			{
				parserState = ParserState.Invalid;
			}
			switch (parserState)
			{
			case ParserState.Done:
				return CreateHttpRequestMessage(uriScheme, httpRequest, stream, num - headerConsumed);
			case ParserState.NeedMoreData:
				continue;
			}
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.HttpMessageParserError, headerConsumed, buffer);
		}
		while (num != 0);
		throw new IOException(System.Net.Http.Properties.Resources.ReadAsHttpMessageUnexpectedTermination);
	}

	public static Task<HttpResponseMessage> ReadAsHttpResponseMessageAsync(this HttpContent content)
	{
		return content.ReadAsHttpResponseMessageAsync(32768);
	}

	public static Task<HttpResponseMessage> ReadAsHttpResponseMessageAsync(this HttpContent content, CancellationToken cancellationToken)
	{
		return content.ReadAsHttpResponseMessageAsync(32768, cancellationToken);
	}

	public static Task<HttpResponseMessage> ReadAsHttpResponseMessageAsync(this HttpContent content, int bufferSize)
	{
		return content.ReadAsHttpResponseMessageAsync(bufferSize, 16384);
	}

	public static Task<HttpResponseMessage> ReadAsHttpResponseMessageAsync(this HttpContent content, int bufferSize, CancellationToken cancellationToken)
	{
		return content.ReadAsHttpResponseMessageAsync(bufferSize, 16384, cancellationToken);
	}

	public static Task<HttpResponseMessage> ReadAsHttpResponseMessageAsync(this HttpContent content, int bufferSize, int maxHeaderSize)
	{
		return content.ReadAsHttpResponseMessageAsync(bufferSize, maxHeaderSize, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> ReadAsHttpResponseMessageAsync(this HttpContent content, int bufferSize, int maxHeaderSize, CancellationToken cancellationToken)
	{
		if (content == null)
		{
			throw Error.ArgumentNull("content");
		}
		if (bufferSize < 256)
		{
			throw Error.ArgumentMustBeGreaterThanOrEqualTo("bufferSize", bufferSize, 256);
		}
		if (maxHeaderSize < 2)
		{
			throw Error.ArgumentMustBeGreaterThanOrEqualTo("maxHeaderSize", maxHeaderSize, 2);
		}
		HttpMessageContent.ValidateHttpMessageContent(content, isRequest: false, throwOnError: true);
		return content.ReadAsHttpResponseMessageAsyncCore(bufferSize, maxHeaderSize, cancellationToken);
	}

	private static async Task<HttpResponseMessage> ReadAsHttpResponseMessageAsyncCore(this HttpContent content, int bufferSize, int maxHeaderSize, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		Stream stream = await content.ReadAsStreamAsync();
		HttpUnsortedResponse httpResponse = new HttpUnsortedResponse();
		HttpResponseHeaderParser parser = new HttpResponseHeaderParser(httpResponse, 2048, maxHeaderSize);
		byte[] buffer = new byte[bufferSize];
		int headerConsumed = 0;
		int num;
		do
		{
			try
			{
				num = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
			}
			catch (Exception innerException)
			{
				throw new IOException(System.Net.Http.Properties.Resources.HttpMessageErrorReading, innerException);
			}
			ParserState parserState;
			try
			{
				parserState = parser.ParseBuffer(buffer, num, ref headerConsumed);
			}
			catch (Exception)
			{
				parserState = ParserState.Invalid;
			}
			switch (parserState)
			{
			case ParserState.Done:
				return CreateHttpResponseMessage(httpResponse, stream, num - headerConsumed);
			case ParserState.NeedMoreData:
				continue;
			}
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.HttpMessageParserError, headerConsumed, buffer);
		}
		while (num != 0);
		throw new IOException(System.Net.Http.Properties.Resources.ReadAsHttpMessageUnexpectedTermination);
	}

	private static Uri CreateRequestUri(string uriScheme, HttpUnsortedRequest httpRequest)
	{
		if (httpRequest.HttpHeaders.TryGetValues("Host", out var values))
		{
			int num = values.Count();
			if (num != 1)
			{
				throw Error.InvalidOperation(System.Net.Http.Properties.Resources.HttpMessageParserInvalidHostCount, "Host", num);
			}
			return new Uri(string.Format(CultureInfo.InvariantCulture, "{0}://{1}{2}", new object[3]
			{
				uriScheme,
				values.ElementAt(0),
				httpRequest.RequestUri
			}));
		}
		throw Error.InvalidOperation(System.Net.Http.Properties.Resources.HttpMessageParserInvalidHostCount, "Host", 0);
	}

	private static HttpContent CreateHeaderFields(HttpHeaders source, HttpHeaders destination, Stream contentStream, int rewind)
	{
		HttpContentHeaders httpContentHeaders = null;
		HttpContent httpContent = null;
		foreach (KeyValuePair<string, IEnumerable<string>> item in source)
		{
			if (!destination.TryAddWithoutValidation(item.Key, item.Value))
			{
				if (httpContentHeaders == null)
				{
					httpContentHeaders = FormattingUtilities.CreateEmptyContentHeaders();
				}
				httpContentHeaders.TryAddWithoutValidation(item.Key, item.Value);
			}
		}
		if (httpContentHeaders != null)
		{
			if (!contentStream.CanSeek)
			{
				throw Error.InvalidOperation(System.Net.Http.Properties.Resources.HttpMessageContentStreamMustBeSeekable, "ContentReadStream", FormattingUtilities.HttpResponseMessageType.Name);
			}
			contentStream.Seek(-rewind, SeekOrigin.Current);
			httpContent = new StreamContent(contentStream);
			httpContentHeaders.CopyTo(httpContent.Headers);
		}
		return httpContent;
	}

	private static HttpRequestMessage CreateHttpRequestMessage(string uriScheme, HttpUnsortedRequest httpRequest, Stream contentStream, int rewind)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
		httpRequestMessage.Method = httpRequest.Method;
		httpRequestMessage.RequestUri = CreateRequestUri(uriScheme, httpRequest);
		httpRequestMessage.Version = httpRequest.Version;
		httpRequestMessage.Content = CreateHeaderFields(httpRequest.HttpHeaders, httpRequestMessage.Headers, contentStream, rewind);
		return httpRequestMessage;
	}

	private static HttpResponseMessage CreateHttpResponseMessage(HttpUnsortedResponse httpResponse, Stream contentStream, int rewind)
	{
		HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
		httpResponseMessage.Version = httpResponse.Version;
		httpResponseMessage.StatusCode = httpResponse.StatusCode;
		httpResponseMessage.ReasonPhrase = httpResponse.ReasonPhrase;
		httpResponseMessage.Content = CreateHeaderFields(httpResponse.HttpHeaders, httpResponseMessage.Headers, contentStream, rewind);
		return httpResponseMessage;
	}
}
