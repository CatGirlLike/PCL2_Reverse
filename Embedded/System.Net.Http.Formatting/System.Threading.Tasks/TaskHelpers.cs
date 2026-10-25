using System.Runtime.InteropServices;

namespace System.Threading.Tasks;

internal static class TaskHelpers
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct AsyncVoid
	{
	}

	private static class CancelCache<TResult>
	{
		public static readonly Task<TResult> Canceled = GetCancelledTask();

		private static Task<TResult> GetCancelledTask()
		{
			TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
			taskCompletionSource.SetCanceled();
			return taskCompletionSource.Task;
		}
	}

	private static readonly Task _defaultCompleted = Task.FromResult(default(AsyncVoid));

	private static readonly Task<object> _completedTaskReturningNull = Task.FromResult<object>(null);

	internal static Task Canceled()
	{
		return CancelCache<AsyncVoid>.Canceled;
	}

	internal static Task<TResult> Canceled<TResult>()
	{
		return CancelCache<TResult>.Canceled;
	}

	internal static Task Completed()
	{
		return _defaultCompleted;
	}

	internal static Task FromError(Exception exception)
	{
		return FromError<AsyncVoid>(exception);
	}

	internal static Task<TResult> FromError<TResult>(Exception exception)
	{
		TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
		taskCompletionSource.SetException(exception);
		return taskCompletionSource.Task;
	}

	internal static Task<object> NullResult()
	{
		return _completedTaskReturningNull;
	}
}
