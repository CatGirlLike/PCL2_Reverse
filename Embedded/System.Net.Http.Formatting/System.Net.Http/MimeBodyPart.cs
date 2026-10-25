using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting.Parsers;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http;

internal class MimeBodyPart : IDisposable
{
	private static readonly Type _streamType = typeof(Stream);

	private Stream _outputStream;

	private MultipartStreamProvider _streamProvider;

	private HttpContent _parentContent;

	private HttpContent _content;

	private HttpContentHeaders _headers;

	public InternetMessageFormatHeaderParser HeaderParser { get; private set; }

	public List<ArraySegment<byte>> Segments { get; private set; }

	public bool IsComplete { get; set; }

	public bool IsFinal { get; set; }

	public MimeBodyPart(MultipartStreamProvider streamProvider, int maxBodyPartHeaderSize, HttpContent parentContent)
	{
		_streamProvider = streamProvider;
		_parentContent = parentContent;
		Segments = new List<ArraySegment<byte>>(2);
		_headers = FormattingUtilities.CreateEmptyContentHeaders();
		HeaderParser = new InternetMessageFormatHeaderParser(_headers, maxBodyPartHeaderSize, ignoreHeaderValidation: true);
	}

	public HttpContent GetCompletedHttpContent()
	{
		if (_content == null)
		{
			return null;
		}
		_headers.CopyTo(_content.Headers);
		return _content;
	}

	public async Task WriteSegment(ArraySegment<byte> segment, CancellationToken cancellationToken)
	{
		await GetOutputStream().WriteAsync(segment.Array, segment.Offset, segment.Count, cancellationToken);
	}

	private Stream GetOutputStream()
	{
		if (_outputStream == null)
		{
			try
			{
				_outputStream = _streamProvider.GetStream(_parentContent, _headers);
			}
			catch (Exception innerException)
			{
				throw Error.InvalidOperation(innerException, System.Net.Http.Properties.Resources.ReadAsMimeMultipartStreamProviderException, _streamProvider.GetType().Name);
			}
			if (_outputStream == null)
			{
				throw Error.InvalidOperation(System.Net.Http.Properties.Resources.ReadAsMimeMultipartStreamProviderNull, _streamProvider.GetType().Name, _streamType.Name);
			}
			if (!_outputStream.CanWrite)
			{
				throw Error.InvalidOperation(System.Net.Http.Properties.Resources.ReadAsMimeMultipartStreamProviderReadOnly, _streamProvider.GetType().Name, _streamType.Name);
			}
			_content = new StreamContent(_outputStream);
		}
		return _outputStream;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected void Dispose(bool disposing)
	{
		if (disposing)
		{
			CleanupOutputStream();
			CleanupHttpContent();
			_parentContent = null;
			HeaderParser = null;
			Segments.Clear();
		}
	}

	private void CleanupHttpContent()
	{
		if (!IsComplete && _content != null)
		{
			_content.Dispose();
		}
		_content = null;
	}

	private void CleanupOutputStream()
	{
		if (_outputStream != null)
		{
			if (_outputStream is MemoryStream memoryStream)
			{
				memoryStream.Position = 0L;
			}
			else
			{
				_outputStream.Close();
			}
			_outputStream = null;
		}
	}
}
