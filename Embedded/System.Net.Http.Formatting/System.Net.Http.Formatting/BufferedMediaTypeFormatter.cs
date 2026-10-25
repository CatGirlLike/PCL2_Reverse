using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Internal;
using System.Net.Http.Properties;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http.Formatting;

public abstract class BufferedMediaTypeFormatter : MediaTypeFormatter
{
	private const int MinBufferSize = 0;

	private const int DefaultBufferSize = 16384;

	private int _bufferSizeInBytes = 16384;

	public int BufferSize
	{
		get
		{
			return _bufferSizeInBytes;
		}
		set
		{
			if (value < 0)
			{
				throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 0);
			}
			_bufferSizeInBytes = value;
		}
	}

	protected BufferedMediaTypeFormatter()
	{
	}

	protected BufferedMediaTypeFormatter(BufferedMediaTypeFormatter formatter)
		: base(formatter)
	{
		BufferSize = formatter.BufferSize;
	}

	public virtual void WriteToStream(Type type, object value, Stream writeStream, HttpContent content, CancellationToken cancellationToken)
	{
		WriteToStream(type, value, writeStream, content);
	}

	public virtual void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
	{
		throw Error.NotSupported(System.Net.Http.Properties.Resources.MediaTypeFormatterCannotWriteSync, GetType().Name);
	}

	public virtual object ReadFromStream(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
	{
		return ReadFromStream(type, readStream, content, formatterLogger);
	}

	public virtual object ReadFromStream(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
	{
		throw Error.NotSupported(System.Net.Http.Properties.Resources.MediaTypeFormatterCannotReadSync, GetType().Name);
	}

	public sealed override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
	{
		return WriteToStreamAsync(type, value, writeStream, content, transportContext, CancellationToken.None);
	}

	public sealed override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext, CancellationToken cancellationToken)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (writeStream == null)
		{
			throw Error.ArgumentNull("writeStream");
		}
		try
		{
			WriteToStreamSync(type, value, writeStream, content, cancellationToken);
			return TaskHelpers.Completed();
		}
		catch (Exception exception)
		{
			return TaskHelpers.FromError(exception);
		}
	}

	private void WriteToStreamSync(Type type, object value, Stream writeStream, HttpContent content, CancellationToken cancellationToken)
	{
		using Stream writeStream2 = GetBufferStream(writeStream, _bufferSizeInBytes);
		WriteToStream(type, value, writeStream2, content, cancellationToken);
	}

	public sealed override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
	{
		return ReadFromStreamAsync(type, readStream, content, formatterLogger, CancellationToken.None);
	}

	public sealed override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (readStream == null)
		{
			throw Error.ArgumentNull("readStream");
		}
		try
		{
			return Task.FromResult(ReadFromStreamSync(type, readStream, content, formatterLogger, cancellationToken));
		}
		catch (Exception exception)
		{
			return TaskHelpers.FromError<object>(exception);
		}
	}

	private object ReadFromStreamSync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
	{
		HttpContentHeaders httpContentHeaders = content?.Headers;
		if (httpContentHeaders != null && httpContentHeaders.ContentLength == 0)
		{
			return MediaTypeFormatter.GetDefaultValueForType(type);
		}
		using Stream readStream2 = GetBufferStream(readStream, _bufferSizeInBytes);
		return ReadFromStream(type, readStream2, content, formatterLogger, cancellationToken);
	}

	private static Stream GetBufferStream(Stream innerStream, int bufferSize)
	{
		return new BufferedStream(new NonClosingDelegatingStream(innerStream), bufferSize);
	}
}
