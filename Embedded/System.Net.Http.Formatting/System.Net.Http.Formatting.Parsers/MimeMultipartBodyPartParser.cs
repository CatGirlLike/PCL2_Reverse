using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Web.Http;

namespace System.Net.Http.Formatting.Parsers;

internal class MimeMultipartBodyPartParser : IDisposable
{
	internal const long DefaultMaxMessageSize = long.MaxValue;

	private const int DefaultMaxBodyPartHeaderSize = 4096;

	private MimeMultipartParser _mimeParser;

	private MimeMultipartParser.State _mimeStatus;

	private ArraySegment<byte>[] _parsedBodyPart = new ArraySegment<byte>[2];

	private MimeBodyPart _currentBodyPart;

	private bool _isFirst = true;

	private ParserState _bodyPartHeaderStatus;

	private int _maxBodyPartHeaderSize;

	private MultipartStreamProvider _streamProvider;

	private HttpContent _content;

	public MimeMultipartBodyPartParser(HttpContent content, MultipartStreamProvider streamProvider)
		: this(content, streamProvider, long.MaxValue, 4096)
	{
	}

	public MimeMultipartBodyPartParser(HttpContent content, MultipartStreamProvider streamProvider, long maxMessageSize, int maxBodyPartHeaderSize)
	{
		string boundary = ValidateArguments(content, maxMessageSize, throwOnError: true);
		_mimeParser = new MimeMultipartParser(boundary, maxMessageSize);
		_currentBodyPart = new MimeBodyPart(streamProvider, maxBodyPartHeaderSize, content);
		_content = content;
		_maxBodyPartHeaderSize = maxBodyPartHeaderSize;
		_streamProvider = streamProvider;
	}

	public static bool IsMimeMultipartContent(HttpContent content)
	{
		try
		{
			return ValidateArguments(content, long.MaxValue, throwOnError: false) != null;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public IEnumerable<MimeBodyPart> ParseBuffer(byte[] data, int bytesRead)
	{
		int bytesConsumed = 0;
		bool isFinalBodyPart = false;
		if (bytesRead == 0 && !_mimeParser.IsWaitingForEndOfMessage)
		{
			CleanupCurrentBodyPart();
			throw new IOException(System.Net.Http.Properties.Resources.ReadAsMimeMultipartUnexpectedTermination);
		}
		_currentBodyPart.Segments.Clear();
		while (_mimeParser.CanParseMore(bytesRead, bytesConsumed))
		{
			_mimeStatus = _mimeParser.ParseBuffer(data, bytesRead, ref bytesConsumed, out _parsedBodyPart[0], out _parsedBodyPart[1], out isFinalBodyPart);
			if (_mimeStatus != MimeMultipartParser.State.BodyPartCompleted && _mimeStatus != 0)
			{
				CleanupCurrentBodyPart();
				throw Error.InvalidOperation(System.Net.Http.Properties.Resources.ReadAsMimeMultipartParseError, bytesConsumed, data);
			}
			if (_isFirst)
			{
				if (_mimeStatus == MimeMultipartParser.State.BodyPartCompleted)
				{
					_isFirst = false;
				}
				continue;
			}
			ArraySegment<byte>[] parsedBodyPart = _parsedBodyPart;
			for (int i = 0; i < parsedBodyPart.Length; i++)
			{
				ArraySegment<byte> item = parsedBodyPart[i];
				if (item.Count == 0)
				{
					continue;
				}
				if (_bodyPartHeaderStatus != ParserState.Done)
				{
					int bytesConsumed2 = item.Offset;
					_bodyPartHeaderStatus = _currentBodyPart.HeaderParser.ParseBuffer(item.Array, item.Count + item.Offset, ref bytesConsumed2);
					if (_bodyPartHeaderStatus == ParserState.Done)
					{
						_currentBodyPart.Segments.Add(new ArraySegment<byte>(item.Array, bytesConsumed2, item.Count + item.Offset - bytesConsumed2));
					}
					else if (_bodyPartHeaderStatus != 0)
					{
						CleanupCurrentBodyPart();
						throw Error.InvalidOperation(System.Net.Http.Properties.Resources.ReadAsMimeMultipartHeaderParseError, bytesConsumed2, item.Array);
					}
				}
				else
				{
					_currentBodyPart.Segments.Add(item);
				}
			}
			if (_mimeStatus == MimeMultipartParser.State.BodyPartCompleted)
			{
				MimeBodyPart currentBodyPart = _currentBodyPart;
				currentBodyPart.IsComplete = true;
				currentBodyPart.IsFinal = isFinalBodyPart;
				_currentBodyPart = new MimeBodyPart(_streamProvider, _maxBodyPartHeaderSize, _content);
				_mimeStatus = MimeMultipartParser.State.NeedMoreData;
				_bodyPartHeaderStatus = ParserState.NeedMoreData;
				yield return currentBodyPart;
			}
			else
			{
				yield return _currentBodyPart;
			}
		}
	}

	protected void Dispose(bool disposing)
	{
		if (disposing)
		{
			_mimeParser = null;
			CleanupCurrentBodyPart();
		}
	}

	private static string ValidateArguments(HttpContent content, long maxMessageSize, bool throwOnError)
	{
		if (maxMessageSize < 10)
		{
			if (throwOnError)
			{
				throw Error.ArgumentMustBeGreaterThanOrEqualTo("maxMessageSize", maxMessageSize, 10);
			}
			return null;
		}
		MediaTypeHeaderValue contentType = content.Headers.ContentType;
		if (contentType == null)
		{
			if (throwOnError)
			{
				throw Error.Argument("content", System.Net.Http.Properties.Resources.ReadAsMimeMultipartArgumentNoContentType, typeof(HttpContent).Name, "multipart/");
			}
			return null;
		}
		if (!contentType.MediaType.StartsWith("multipart", StringComparison.OrdinalIgnoreCase))
		{
			if (throwOnError)
			{
				throw Error.Argument("content", System.Net.Http.Properties.Resources.ReadAsMimeMultipartArgumentNoMultipart, typeof(HttpContent).Name, "multipart/");
			}
			return null;
		}
		string text = null;
		foreach (NameValueHeaderValue parameter in contentType.Parameters)
		{
			if (parameter.Name.Equals("boundary", StringComparison.OrdinalIgnoreCase))
			{
				text = FormattingUtilities.UnquoteToken(parameter.Value);
				break;
			}
		}
		if (text == null)
		{
			if (throwOnError)
			{
				throw Error.Argument("content", System.Net.Http.Properties.Resources.ReadAsMimeMultipartArgumentNoBoundary, typeof(HttpContent).Name, "multipart", "boundary");
			}
			return null;
		}
		return text;
	}

	private void CleanupCurrentBodyPart()
	{
		if (_currentBodyPart != null)
		{
			_currentBodyPart.Dispose();
			_currentBodyPart = null;
		}
	}
}
