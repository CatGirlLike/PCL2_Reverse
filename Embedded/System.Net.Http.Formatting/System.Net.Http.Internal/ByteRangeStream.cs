using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http.Internal;

internal class ByteRangeStream : DelegatingStream
{
	private readonly long _lowerbounds;

	private readonly long _totalCount;

	private long _currentCount;

	public ContentRangeHeaderValue ContentRange { get; private set; }

	public override long Length => _totalCount;

	public override bool CanWrite => false;

	public override long Position
	{
		get
		{
			return _currentCount;
		}
		set
		{
			if (value < 0)
			{
				throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 0L);
			}
			_currentCount = value;
		}
	}

	public ByteRangeStream(Stream innerStream, RangeItemHeaderValue range)
		: base(innerStream)
	{
		if (range == null)
		{
			throw Error.ArgumentNull("range");
		}
		if (!innerStream.CanSeek)
		{
			throw Error.Argument("innerStream", System.Net.Http.Properties.Resources.ByteRangeStreamNotSeekable, typeof(ByteRangeStream).Name);
		}
		if (innerStream.Length < 1)
		{
			throw Error.ArgumentOutOfRange("innerStream", innerStream.Length, System.Net.Http.Properties.Resources.ByteRangeStreamEmpty, typeof(ByteRangeStream).Name);
		}
		if (range.From.HasValue && range.From.Value > innerStream.Length)
		{
			throw Error.ArgumentOutOfRange("range", range.From, System.Net.Http.Properties.Resources.ByteRangeStreamInvalidFrom, innerStream.Length);
		}
		long num = innerStream.Length - 1;
		long num2;
		if (range.To.HasValue)
		{
			if (range.From.HasValue)
			{
				num2 = Math.Min(range.To.Value, num);
				_lowerbounds = range.From.Value;
			}
			else
			{
				num2 = num;
				_lowerbounds = Math.Max(innerStream.Length - range.To.Value, 0L);
			}
		}
		else if (range.From.HasValue)
		{
			num2 = num;
			_lowerbounds = range.From.Value;
		}
		else
		{
			num2 = num;
			_lowerbounds = 0L;
		}
		_totalCount = num2 - _lowerbounds + 1;
		ContentRange = new ContentRangeHeaderValue(_lowerbounds, num2, innerStream.Length);
	}

	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		return base.BeginRead(buffer, offset, PrepareStreamForRangeRead(count), callback, state);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return base.Read(buffer, offset, PrepareStreamForRangeRead(count));
	}

	public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		return base.ReadAsync(buffer, offset, PrepareStreamForRangeRead(count), cancellationToken);
	}

	public override int ReadByte()
	{
		if (PrepareStreamForRangeRead(1) <= 0)
		{
			return -1;
		}
		return base.ReadByte();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		switch (origin)
		{
		case SeekOrigin.Begin:
			_currentCount = offset;
			break;
		case SeekOrigin.Current:
			_currentCount += offset;
			break;
		case SeekOrigin.End:
			_currentCount = _totalCount + offset;
			break;
		default:
			throw Error.InvalidEnumArgument("origin", (int)origin, typeof(SeekOrigin));
		}
		if (_currentCount < 0)
		{
			throw new IOException(System.Net.Http.Properties.Resources.ByteRangeStreamInvalidOffset);
		}
		return _currentCount;
	}

	public override void SetLength(long value)
	{
		throw Error.NotSupported(System.Net.Http.Properties.Resources.ByteRangeStreamReadOnly);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw Error.NotSupported(System.Net.Http.Properties.Resources.ByteRangeStreamReadOnly);
	}

	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		throw Error.NotSupported(System.Net.Http.Properties.Resources.ByteRangeStreamReadOnly);
	}

	public override void EndWrite(IAsyncResult asyncResult)
	{
		throw Error.NotSupported(System.Net.Http.Properties.Resources.ByteRangeStreamReadOnly);
	}

	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		throw Error.NotSupported(System.Net.Http.Properties.Resources.ByteRangeStreamReadOnly);
	}

	public override void WriteByte(byte value)
	{
		throw Error.NotSupported(System.Net.Http.Properties.Resources.ByteRangeStreamReadOnly);
	}

	private int PrepareStreamForRangeRead(int count)
	{
		if (count <= 0)
		{
			return count;
		}
		if (_currentCount >= _totalCount)
		{
			return 0;
		}
		long num = Math.Min(count, _totalCount - _currentCount);
		long num2 = _lowerbounds + _currentCount;
		long position = base.InnerStream.Position;
		if (num2 != position)
		{
			base.InnerStream.Position = num2;
		}
		_currentCount += num;
		return (int)num;
	}
}
