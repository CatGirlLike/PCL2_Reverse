using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Internal;
using System.Net.Http.Properties;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http;

public class ByteRangeStreamContent : HttpContent
{
	private const string SupportedRangeUnit = "bytes";

	private const string ByteRangesContentSubtype = "byteranges";

	private const int DefaultBufferSize = 4096;

	private const int MinBufferSize = 1;

	private readonly Stream _content;

	private readonly long _start;

	private readonly HttpContent _byteRangeContent;

	private bool _disposed;

	public ByteRangeStreamContent(Stream content, RangeHeaderValue range, string mediaType)
		: this(content, range, new MediaTypeHeaderValue(mediaType), 4096)
	{
	}

	public ByteRangeStreamContent(Stream content, RangeHeaderValue range, string mediaType, int bufferSize)
		: this(content, range, new MediaTypeHeaderValue(mediaType), bufferSize)
	{
	}

	public ByteRangeStreamContent(Stream content, RangeHeaderValue range, MediaTypeHeaderValue mediaType)
		: this(content, range, mediaType, 4096)
	{
	}

	public ByteRangeStreamContent(Stream content, RangeHeaderValue range, MediaTypeHeaderValue mediaType, int bufferSize)
	{
		if (content == null)
		{
			throw Error.ArgumentNull("content");
		}
		if (!content.CanSeek)
		{
			throw Error.Argument("content", System.Net.Http.Properties.Resources.ByteRangeStreamNotSeekable, typeof(ByteRangeStreamContent).Name);
		}
		if (range == null)
		{
			throw Error.ArgumentNull("range");
		}
		if (mediaType == null)
		{
			throw Error.ArgumentNull("mediaType");
		}
		if (bufferSize < 1)
		{
			throw Error.ArgumentMustBeGreaterThanOrEqualTo("bufferSize", bufferSize, 1);
		}
		if (!range.Unit.Equals("bytes", StringComparison.OrdinalIgnoreCase))
		{
			throw Error.Argument("range", System.Net.Http.Properties.Resources.ByteRangeStreamContentNotBytesRange, range.Unit, "bytes");
		}
		try
		{
			if (range.Ranges.Count > 1)
			{
				MultipartContent multipartContent = (MultipartContent)(_byteRangeContent = new MultipartContent("byteranges"));
				foreach (RangeItemHeaderValue range2 in range.Ranges)
				{
					try
					{
						ByteRangeStream byteRangeStream = new ByteRangeStream(content, range2);
						HttpContent httpContent = new StreamContent(byteRangeStream, bufferSize);
						httpContent.Headers.ContentType = mediaType;
						httpContent.Headers.ContentRange = byteRangeStream.ContentRange;
						multipartContent.Add(httpContent);
					}
					catch (ArgumentOutOfRangeException)
					{
					}
				}
				if (!multipartContent.Any())
				{
					ContentRangeHeaderValue contentRange = new ContentRangeHeaderValue(content.Length);
					string message = Error.Format(System.Net.Http.Properties.Resources.ByteRangeStreamNoneOverlap, range.ToString());
					throw new InvalidByteRangeException(contentRange, message);
				}
			}
			else
			{
				if (range.Ranges.Count != 1)
				{
					throw Error.Argument("range", System.Net.Http.Properties.Resources.ByteRangeStreamContentNoRanges);
				}
				try
				{
					ByteRangeStream byteRangeStream2 = new ByteRangeStream(content, range.Ranges.First());
					_byteRangeContent = new StreamContent(byteRangeStream2, bufferSize);
					_byteRangeContent.Headers.ContentType = mediaType;
					_byteRangeContent.Headers.ContentRange = byteRangeStream2.ContentRange;
				}
				catch (ArgumentOutOfRangeException)
				{
					ContentRangeHeaderValue contentRange2 = new ContentRangeHeaderValue(content.Length);
					string message2 = Error.Format(System.Net.Http.Properties.Resources.ByteRangeStreamNoOverlap, range.ToString());
					throw new InvalidByteRangeException(contentRange2, message2);
				}
			}
			_byteRangeContent.Headers.CopyTo(base.Headers);
			_content = content;
			_start = content.Position;
		}
		catch
		{
			if (_byteRangeContent != null)
			{
				_byteRangeContent.Dispose();
			}
			throw;
		}
	}

	protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
	{
		_content.Position = _start;
		return _byteRangeContent.CopyToAsync(stream);
	}

	protected override bool TryComputeLength(out long length)
	{
		long? contentLength = _byteRangeContent.Headers.ContentLength;
		if (contentLength.HasValue)
		{
			length = contentLength.Value;
			return true;
		}
		length = -1L;
		return false;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && !_disposed)
		{
			_byteRangeContent.Dispose();
			_content.Dispose();
			_disposed = true;
		}
		base.Dispose(disposing);
	}
}
