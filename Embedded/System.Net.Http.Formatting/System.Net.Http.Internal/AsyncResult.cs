using System.Net.Http.Properties;
using System.Threading;
using System.Web.Http;

namespace System.Net.Http.Internal;

internal abstract class AsyncResult : IAsyncResult
{
	private AsyncCallback _callback;

	private object _state;

	private bool _isCompleted;

	private bool _completedSynchronously;

	private bool _endCalled;

	private Exception _exception;

	public object AsyncState => _state;

	public WaitHandle AsyncWaitHandle => null;

	public bool CompletedSynchronously => _completedSynchronously;

	public bool HasCallback => _callback != null;

	public bool IsCompleted => _isCompleted;

	protected AsyncResult(AsyncCallback callback, object state)
	{
		_callback = callback;
		_state = state;
	}

	protected void Complete(bool completedSynchronously)
	{
		if (_isCompleted)
		{
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.AsyncResult_MultipleCompletes, GetType().Name);
		}
		_completedSynchronously = completedSynchronously;
		_isCompleted = true;
		if (_callback != null)
		{
			try
			{
				_callback(this);
			}
			catch (Exception innerException)
			{
				throw Error.InvalidOperation(innerException, System.Net.Http.Properties.Resources.AsyncResult_CallbackThrewException);
			}
		}
	}

	protected void Complete(bool completedSynchronously, Exception exception)
	{
		_exception = exception;
		Complete(completedSynchronously);
	}

	protected static TAsyncResult End<TAsyncResult>(IAsyncResult result) where TAsyncResult : AsyncResult
	{
		if (result == null)
		{
			throw Error.ArgumentNull("result");
		}
		if (!(result is TAsyncResult val))
		{
			throw Error.Argument("result", System.Net.Http.Properties.Resources.AsyncResult_ResultMismatch);
		}
		if (!val._isCompleted)
		{
			val.AsyncWaitHandle.WaitOne();
		}
		if (val._endCalled)
		{
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.AsyncResult_MultipleEnds);
		}
		val._endCalled = true;
		if (val._exception != null)
		{
			throw val._exception;
		}
		return val;
	}
}
