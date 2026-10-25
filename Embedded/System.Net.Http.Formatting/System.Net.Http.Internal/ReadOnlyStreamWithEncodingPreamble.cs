using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http.Internal;

internal class ReadOnlyStreamWithEncodingPreamble : Stream
{
	private static Task<int> _cancelledTask = GetCancelledTask();

	private Stream _innerStream;

	private ArraySegment<byte> _remainingBytes;

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override long Position
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public ReadOnlyStreamWithEncodingPreamble(Stream innerStream, Encoding encoding)
	{
		_innerStream = innerStream;
		byte[] preamble = encoding.GetPreamble();
		int num = preamble.Length;
		if (num <= 0)
		{
			return;
		}
		int num2 = num * 2;
		byte[] array = new byte[num2];
		int i = num;
		preamble.CopyTo(array, 0);
		for (; i < num2; i++)
		{
			int num3 = innerStream.ReadByte();
			if (num3 == -1)
			{
				break;
			}
			array[i] = (byte)num3;
		}
		if (i == num2)
		{
			bool flag = true;
			for (int j = 0; j < num; j++)
			{
				if (array[j] != array[j + num])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				i = num;
			}
		}
		_remainingBytes = new ArraySegment<byte>(array, 0, i);
	}

	public override void Flush()
	{
		throw new NotImplementedException();
	}

	private static Task<int> GetCancelledTask()
	{
		TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
		taskCompletionSource.SetCanceled();
		return taskCompletionSource.Task;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		byte[] array = _remainingBytes.Array;
		if (array == null)
		{
			return _innerStream.Read(buffer, offset, count);
		}
		int count2 = _remainingBytes.Count;
		int offset2 = _remainingBytes.Offset;
		int num = Math.Min(count, count2);
		for (int i = 0; i < num; i++)
		{
			buffer[offset + i] = array[offset2 + i];
		}
		if (num == count2)
		{
			_remainingBytes = default(ArraySegment<byte>);
		}
		else
		{
			_remainingBytes = new ArraySegment<byte>(array, offset2 + num, count2 - num);
		}
		return num;
	}

	public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		if (_remainingBytes.Array == null)
		{
			return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return _cancelledTask;
		}
		return Task.FromResult(Read(buffer, offset, count));
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}
}
