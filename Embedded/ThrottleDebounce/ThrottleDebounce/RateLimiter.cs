using System;
using System.Reflection;
using System.Threading;
using System.Timers;

namespace ThrottleDebounce;

internal class RateLimiter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> : RateLimitedFunc<TResult>, IDisposable, RateLimitedFunc<T1, TResult>, RateLimitedFunc<T1, T2, TResult>, RateLimitedFunc<T1, T2, T3, TResult>, RateLimitedFunc<T1, T2, T3, T4, TResult>, RateLimitedFunc<T1, T2, T3, T4, T5, TResult>, RateLimitedFunc<T1, T2, T3, T4, T5, T6, TResult>, RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, TResult>, RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>, RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>, RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>, RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>, RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>, RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>, RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>, RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>, RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>, RateLimitedAction, RateLimitedAction<T1>, RateLimitedAction<T1, T2>, RateLimitedAction<T1, T2, T3>, RateLimitedAction<T1, T2, T3, T4>, RateLimitedAction<T1, T2, T3, T4, T5>, RateLimitedAction<T1, T2, T3, T4, T5, T6>, RateLimitedAction<T1, T2, T3, T4, T5, T6, T7>, RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8>, RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>, RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
{
	private readonly Delegate rateLimitedCallback;

	private readonly bool leading;

	private readonly bool trailing;

	private readonly System.Timers.Timer minTimer;

	private readonly System.Timers.Timer? maxTimer;

	private readonly FixedSizeArrayPool<object> parameterArrayPool;

	private readonly int arity;

	private int queuedInvocations;

	private object[]? mostRecentInvocationParameters;

	private TResult? mostRecentResult;

	private volatile int minTimerRunning;

	private volatile bool disposed;

	internal RateLimiter(Delegate rateLimitedCallback, TimeSpan wait, bool leading, bool trailing, TimeSpan maxWait = default(TimeSpan))
	{
		if (!leading && !trailing)
		{
			throw new ArgumentException("One or both of the leading and trailing arguments must be true, but both were false.");
		}
		TimeSpan zero = TimeSpan.Zero;
		if (zero.Equals(wait))
		{
			throw new ArgumentException("The wait argument must have a positive duration.");
		}
		if (maxWait < TimeSpan.Zero)
		{
			throw new ArgumentException("The maxWait argument must not have a negative duration.");
		}
		this.rateLimitedCallback = rateLimitedCallback;
		this.leading = leading;
		this.trailing = trailing;
		arity = this.rateLimitedCallback.GetMethodInfo().GetParameters().Length;
		parameterArrayPool = ((arity != 0) ? new FixedSizeArrayPool<object>(arity, 2) : null);
		minTimer = new System.Timers.Timer
		{
			AutoReset = false,
			Interval = wait.TotalMilliseconds
		};
		minTimer.Elapsed += delegate
		{
			minTimerRunning = 0;
			WaitTimeHasElapsed();
		};
		if (maxWait != TimeSpan.Zero)
		{
			maxTimer = new System.Timers.Timer
			{
				AutoReset = false,
				Interval = maxWait.TotalMilliseconds
			};
			maxTimer.Elapsed += delegate
			{
				WaitTimeHasElapsed();
			};
		}
	}

	private void WaitTimeHasElapsed()
	{
		if (disposed || Interlocked.Exchange(ref queuedInvocations, 0) <= 0)
		{
			return;
		}
		object[] array = ((arity != 0) ? Interlocked.Exchange(ref mostRecentInvocationParameters, null) : Throttler.NO_PARAMS);
		if (array != null)
		{
			mostRecentResult = (TResult)rateLimitedCallback.DynamicInvoke(array);
			if (arity != 0)
			{
				parameterArrayPool.Return(array);
			}
			resetTimers();
		}
	}

	private TResult? OnUserInvocation(object[] arguments)
	{
		if (!disposed)
		{
			if (arity != 0)
			{
				object[] array = Interlocked.Exchange(ref mostRecentInvocationParameters, arguments);
				if (array != null)
				{
					parameterArrayPool.Return(array);
				}
			}
			bool flag = Interlocked.Exchange(ref minTimerRunning, 1) != 0;
			if (leading && !flag)
			{
				mostRecentResult = (TResult)rateLimitedCallback.DynamicInvoke(arguments);
			}
			else if (trailing)
			{
				Interlocked.Add(ref queuedInvocations, 1);
			}
			resetTimers();
		}
		return mostRecentResult;
	}

	private void resetTimers()
	{
		try
		{
			minTimer.Interval = minTimer.Interval;
			minTimer.Start();
			minTimerRunning = 1;
			maxTimer?.Start();
		}
		catch (ObjectDisposedException)
		{
		}
	}

	public void Dispose()
	{
		disposed = true;
		minTimer.Dispose();
		maxTimer?.Dispose();
		queuedInvocations = 0;
		minTimerRunning = 0;
		mostRecentResult = default(TResult);
		mostRecentInvocationParameters = null;
	}

	void RateLimitedAction.Invoke()
	{
		OnUserInvocation(Throttler.NO_PARAMS);
	}

	TResult? RateLimitedFunc<TResult>.Invoke()
	{
		return OnUserInvocation(Throttler.NO_PARAMS);
	}

	void RateLimitedAction<T1>.Invoke(T1 arg1)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2>.Invoke(T1 arg1, T2 arg2)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2, T3>.Invoke(T1 arg1, T2 arg2, T3 arg3)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2, T3, T4>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2, T3, T4, T5>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2, T3, T4, T5, T6>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2, T3, T4, T5, T6, T7>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		array[12] = arg13;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		array[12] = arg13;
		array[13] = arg14;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		array[12] = arg13;
		array[13] = arg14;
		array[14] = arg15;
		OnUserInvocation(array);
	}

	void RateLimitedAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		array[12] = arg13;
		array[13] = arg14;
		array[14] = arg15;
		array[15] = arg16;
		OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, TResult>.Invoke(T1 arg1)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, TResult>.Invoke(T1 arg1, T2 arg2)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, T3, TResult>.Invoke(T1 arg1, T2 arg2, T3 arg3)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, T3, T4, TResult>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, T3, T4, T5, TResult>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, T3, T4, T5, T6, TResult>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, TResult>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		array[12] = arg13;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		array[12] = arg13;
		array[13] = arg14;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		array[12] = arg13;
		array[13] = arg14;
		array[14] = arg15;
		return OnUserInvocation(array);
	}

	TResult? RateLimitedFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>.Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
	{
		object[] array = parameterArrayPool.Borrow();
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		array[12] = arg13;
		array[13] = arg14;
		array[14] = arg15;
		array[15] = arg16;
		return OnUserInvocation(array);
	}
}
