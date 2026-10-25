using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottleDebounce;

public static class Retrier
{
	private static readonly TimeSpan MaxDelay = TimeSpan.FromMilliseconds((Environment.Version.Major >= 6) ? 4294967294u : 2147483647u);

	public static void Attempt(Action<int> action, uint? maxAttempts = 2u, Func<int, TimeSpan>? delay = null, Func<Exception, bool>? isRetryAllowed = null, Action? beforeRetry = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		int i;
		for (i = 0; !maxAttempts.HasValue || i < maxAttempts - 1; i++)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}
			try
			{
				action(i);
				return;
			}
			catch (Exception ex) when (!(ex is OutOfMemoryException))
			{
				if (isRetryAllowed != null && !isRetryAllowed(ex))
				{
					throw;
				}
				TimeSpan? delay2 = GetDelay(delay, i);
				if (delay2.HasValue)
				{
					TimeSpan valueOrDefault = delay2.GetValueOrDefault();
					Task.Delay(valueOrDefault, cancellationToken).GetAwaiter().GetResult();
				}
				beforeRetry?.Invoke();
			}
		}
		if (!cancellationToken.IsCancellationRequested)
		{
			action(i);
			return;
		}
		throw new TaskCanceledException("Remaining Retrier attempts were cancelled");
	}

	public static T Attempt<T>(Func<int, T> func, uint? maxAttempts = 2u, Func<int, TimeSpan>? delay = null, Func<Exception, bool>? isRetryAllowed = null, Action? beforeRetry = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		int i;
		for (i = 0; !maxAttempts.HasValue || i < maxAttempts - 1; i++)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}
			try
			{
				return func(i);
			}
			catch (Exception ex) when (!(ex is OutOfMemoryException))
			{
				if (isRetryAllowed != null && !isRetryAllowed(ex))
				{
					throw;
				}
				TimeSpan? delay2 = GetDelay(delay, i);
				if (delay2.HasValue)
				{
					TimeSpan valueOrDefault = delay2.GetValueOrDefault();
					Task.Delay(valueOrDefault, cancellationToken).GetAwaiter().GetResult();
				}
				beforeRetry?.Invoke();
			}
		}
		if (!cancellationToken.IsCancellationRequested)
		{
			return func(i);
		}
		throw new TaskCanceledException("Remaining Retrier attempts were cancelled");
	}

	public static async Task Attempt(Func<int, Task> func, uint? maxAttempts = 2u, Func<int, TimeSpan>? delay = null, Func<Exception, bool>? isRetryAllowed = null, Func<Task>? beforeRetry = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		int attempt;
		for (attempt = 0; !maxAttempts.HasValue || attempt < maxAttempts - 1; attempt++)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}
			try
			{
				await func(attempt).ConfigureAwait(continueOnCapturedContext: false);
				return;
			}
			catch (Exception ex) when (!(ex is OutOfMemoryException))
			{
				if (isRetryAllowed != null && !isRetryAllowed(ex))
				{
					throw;
				}
				TimeSpan? delay2 = GetDelay(delay, attempt);
				if (delay2.HasValue)
				{
					TimeSpan valueOrDefault = delay2.GetValueOrDefault();
					await Task.Delay(valueOrDefault, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				beforeRetry?.Invoke();
			}
		}
		if (!cancellationToken.IsCancellationRequested)
		{
			await func(attempt).ConfigureAwait(continueOnCapturedContext: false);
			return;
		}
		throw new TaskCanceledException("Remaining Retrier attempts were cancelled");
	}

	public static async Task<T> Attempt<T>(Func<int, Task<T>> func, uint? maxAttempts = 2u, Func<int, TimeSpan>? delay = null, Func<Exception, bool>? isRetryAllowed = null, Func<Task>? beforeRetry = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		int attempt;
		for (attempt = 0; !maxAttempts.HasValue || attempt < maxAttempts - 1; attempt++)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}
			try
			{
				return await func(attempt).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (Exception ex) when (!(ex is OutOfMemoryException))
			{
				if (isRetryAllowed != null && !isRetryAllowed(ex))
				{
					throw;
				}
				TimeSpan? delay2 = GetDelay(delay, attempt);
				if (delay2.HasValue)
				{
					TimeSpan valueOrDefault = delay2.GetValueOrDefault();
					await Task.Delay(valueOrDefault, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				beforeRetry?.Invoke();
			}
		}
		if (!cancellationToken.IsCancellationRequested)
		{
			return await func(attempt).ConfigureAwait(continueOnCapturedContext: false);
		}
		throw new TaskCanceledException("Remaining Retrier attempts were cancelled");
	}

	private static TimeSpan? GetDelay(Func<int, TimeSpan>? delay, int attempt)
	{
		TimeSpan? timeSpan = delay?.Invoke(attempt);
		if (timeSpan.HasValue)
		{
			TimeSpan valueOrDefault = timeSpan.GetValueOrDefault();
			if (valueOrDefault <= TimeSpan.Zero)
			{
				return null;
			}
			if (valueOrDefault > MaxDelay)
			{
				return MaxDelay;
			}
			return valueOrDefault;
		}
		return null;
	}
}
