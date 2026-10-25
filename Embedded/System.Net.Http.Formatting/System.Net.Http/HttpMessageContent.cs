using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http;

public class HttpMessageContent : HttpContent
{
	private const string SP = " ";

	private const string ColonSP = ": ";

	private const string CRLF = "\r\n";

	private const string CommaSeparator = ", ";

	private const int DefaultHeaderAllocation = 2048;

	private const string DefaultMediaType = "application/http";

	private const string MsgTypeParameter = "msgtype";

	private const string DefaultRequestMsgType = "request";

	private const string DefaultResponseMsgType = "response";

	private const string DefaultRequestMediaType = "application/http; msgtype=request";

	private const string DefaultResponseMediaType = "application/http; msgtype=response";

	private static readonly HashSet<string> _singleValueHeaderFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Cookie", "Set-Cookie", "X-Powered-By" };

	private static readonly HashSet<string> _spaceSeparatedValueHeaderFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "User-Agent" };

	private bool _contentConsumed;

	private Lazy<Task<Stream>> _streamTask;

	private HttpContent Content
	{
		get
		{
			if (HttpRequestMessage == null)
			{
				return HttpResponseMessage.Content;
			}
			return HttpRequestMessage.Content;
		}
	}

	public HttpRequestMessage HttpRequestMessage { get; private set; }

	public HttpResponseMessage HttpResponseMessage { get; private set; }

	public HttpMessageContent(HttpRequestMessage httpRequest)
	{
		if (httpRequest == null)
		{
			throw Error.ArgumentNull("httpRequest");
		}
		HttpRequestMessage = httpRequest;
		base.Headers.ContentType = new MediaTypeHeaderValue("application/http");
		base.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("msgtype", "request"));
		InitializeStreamTask();
	}

	public HttpMessageContent(HttpResponseMessage httpResponse)
	{
		if (httpResponse == null)
		{
			throw Error.ArgumentNull("httpResponse");
		}
		HttpResponseMessage = httpResponse;
		base.Headers.ContentType = new MediaTypeHeaderValue("application/http");
		base.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("msgtype", "response"));
		InitializeStreamTask();
	}

	private void InitializeStreamTask()
	{
		_streamTask = new Lazy<Task<Stream>>(() => (Content != null) ? Content.ReadAsStreamAsync() : null);
	}

	internal static bool ValidateHttpMessageContent(HttpContent content, bool isRequest, bool throwOnError)
	{
		if (content == null)
		{
			throw Error.ArgumentNull("content");
		}
		MediaTypeHeaderValue contentType = content.Headers.ContentType;
		if (contentType != null)
		{
			if (!contentType.MediaType.Equals("application/http", StringComparison.OrdinalIgnoreCase))
			{
				if (throwOnError)
				{
					throw Error.Argument("content", System.Net.Http.Properties.Resources.HttpMessageInvalidMediaType, FormattingUtilities.HttpContentType.Name, isRequest ? "application/http; msgtype=request" : "application/http; msgtype=response");
				}
				return false;
			}
			foreach (NameValueHeaderValue parameter in contentType.Parameters)
			{
				if (!parameter.Name.Equals("msgtype", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				if (!FormattingUtilities.UnquoteToken(parameter.Value).Equals(isRequest ? "request" : "response", StringComparison.OrdinalIgnoreCase))
				{
					if (throwOnError)
					{
						throw Error.Argument("content", System.Net.Http.Properties.Resources.HttpMessageInvalidMediaType, FormattingUtilities.HttpContentType.Name, isRequest ? "application/http; msgtype=request" : "application/http; msgtype=response");
					}
					return false;
				}
				return true;
			}
		}
		if (throwOnError)
		{
			throw Error.Argument("content", System.Net.Http.Properties.Resources.HttpMessageInvalidMediaType, FormattingUtilities.HttpContentType.Name, isRequest ? "application/http; msgtype=request" : "application/http; msgtype=response");
		}
		return false;
	}

	protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
	{
		if (stream == null)
		{
			throw Error.ArgumentNull("stream");
		}
		byte[] array = SerializeHeader();
		await stream.WriteAsync(array, 0, array.Length);
		if (Content != null)
		{
			ValidateStreamForReading(await _streamTask.Value);
			await Content.CopyToAsync(stream);
		}
	}

	protected override bool TryComputeLength(out long length)
	{
		bool num = _streamTask.Value != null;
		length = 0L;
		if (num)
		{
			if (!_streamTask.Value.TryGetResult(out var result) || result == null || !result.CanSeek)
			{
				length = -1L;
				return false;
			}
			length = result.Length;
		}
		byte[] array = SerializeHeader();
		length += array.Length;
		return true;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (HttpRequestMessage != null)
			{
				HttpRequestMessage.Dispose();
				HttpRequestMessage = null;
			}
			if (HttpResponseMessage != null)
			{
				HttpResponseMessage.Dispose();
				HttpResponseMessage = null;
			}
		}
		base.Dispose(disposing);
	}

	private static void SerializeRequestLine(StringBuilder message, HttpRequestMessage httpRequest)
	{
		message.Append(string.Concat(httpRequest.Method, " "));
		message.Append(httpRequest.RequestUri.PathAndQuery + " ");
		message.Append("HTTP/" + ((httpRequest.Version != null) ? httpRequest.Version.ToString(2) : "1.1") + "\r\n");
		if (httpRequest.Headers.Host == null)
		{
			message.Append("Host: " + httpRequest.RequestUri.Authority + "\r\n");
		}
	}

	private static void SerializeStatusLine(StringBuilder message, HttpResponseMessage httpResponse)
	{
		message.Append("HTTP/" + ((httpResponse.Version != null) ? httpResponse.Version.ToString(2) : "1.1") + " ");
		message.Append((int)httpResponse.StatusCode + " ");
		message.Append(httpResponse.ReasonPhrase + "\r\n");
	}

	private static void SerializeHeaderFields(StringBuilder message, HttpHeaders headers)
	{
		if (headers == null)
		{
			return;
		}
		foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
		{
			if (_singleValueHeaderFields.Contains(header.Key))
			{
				foreach (string item in header.Value)
				{
					message.Append(header.Key + ": " + item + "\r\n");
				}
			}
			else if (_spaceSeparatedValueHeaderFields.Contains(header.Key))
			{
				message.Append(header.Key + ": " + string.Join(" ", header.Value) + "\r\n");
			}
			else
			{
				message.Append(header.Key + ": " + string.Join(", ", header.Value) + "\r\n");
			}
		}
	}

	private byte[] SerializeHeader()
	{
		StringBuilder stringBuilder = new StringBuilder(2048);
		HttpHeaders httpHeaders = null;
		HttpContent httpContent = null;
		if (HttpRequestMessage != null)
		{
			SerializeRequestLine(stringBuilder, HttpRequestMessage);
			httpHeaders = HttpRequestMessage.Headers;
			httpContent = HttpRequestMessage.Content;
		}
		else
		{
			SerializeStatusLine(stringBuilder, HttpResponseMessage);
			httpHeaders = HttpResponseMessage.Headers;
			httpContent = HttpResponseMessage.Content;
		}
		SerializeHeaderFields(stringBuilder, httpHeaders);
		if (httpContent != null)
		{
			SerializeHeaderFields(stringBuilder, httpContent.Headers);
		}
		stringBuilder.Append("\r\n");
		return Encoding.UTF8.GetBytes(stringBuilder.ToString());
	}

	private void ValidateStreamForReading(Stream stream)
	{
		if (_contentConsumed)
		{
			if (stream == null || !stream.CanRead)
			{
				throw Error.InvalidOperation(System.Net.Http.Properties.Resources.HttpMessageContentAlreadyRead, FormattingUtilities.HttpContentType.Name, (HttpRequestMessage != null) ? FormattingUtilities.HttpRequestMessageType.Name : FormattingUtilities.HttpResponseMessageType.Name);
			}
			stream.Position = 0L;
		}
		_contentConsumed = true;
	}
}
