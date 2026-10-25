using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ThrottleDebounce;

internal class FixedSizeArrayPool<T>
{
	[CompilerGenerated]
	private int _003CarrayLength_003EP;

	[CompilerGenerated]
	private int _003CpreferredPoolSize_003EP;

	private readonly ConcurrentStack<T[]> _available;

	private int BorrowedCount;

	private int AvailableCount => _available.Count;

	public FixedSizeArrayPool(int arrayLength, int preferredPoolSize)
	{
		_003CarrayLength_003EP = arrayLength;
		_003CpreferredPoolSize_003EP = preferredPoolSize;
		_available = new ConcurrentStack<T[]>();
		base._002Ector();
	}

	public T[] Borrow()
	{
		if (!_available.TryPop(out T[] result))
		{
			result = new T[_003CarrayLength_003EP];
		}
		Interlocked.Increment(ref BorrowedCount);
		return result;
	}

	public void Return(T[] borrowed)
	{
		int num = Interlocked.Decrement(ref BorrowedCount);
		while (num < 0 && num != Interlocked.CompareExchange(ref BorrowedCount, 0, num))
		{
			num = BorrowedCount;
		}
		if (num + AvailableCount < _003CpreferredPoolSize_003EP)
		{
			_available.Push(borrowed);
		}
	}
}
