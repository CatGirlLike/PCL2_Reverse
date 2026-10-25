using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MeloongCore.Extensions;

namespace MeloongCore;

public abstract class RedoableWorkerBase<TOut> : IWorker
{
	[CompilerGenerated]
	private Func<CancellationToken?, ProgressProvider?, TOut> _003Cworkload_003EP;

	[CompilerGenerated]
	private string _003CcreatorMemberName_003EP;

	private bool running;

	private bool hasSucceeded;

	private bool lastSucceeded;

	private object? lastResult;

	private bool pendingRedo;

	private CancellationTokenSource? realCts;

	private CancellationToken lastToken;

	private readonly ManualResetEventSlim idleEvent;

	public bool Running
	{
		get
		{
			lock (this)
			{
				return running;
			}
		}
	}

	public bool HasSucceeded
	{
		get
		{
			lock (this)
			{
				return hasSucceeded;
			}
		}
	}

	public bool LastSucceeded
	{
		get
		{
			lock (this)
			{
				return lastSucceeded;
			}
		}
	}

	public TOut LastResult
	{
		get
		{
			lock (this)
			{
				if (typeof(TOut) == typeof(NoType))
				{
					throw new InvalidOperationException("该 Worker 没有返回值。");
				}
				if (!hasSucceeded)
				{
					throw new InvalidOperationException("从未成功完成过。");
				}
				return (lastResult == null) ? default(TOut) : ((TOut)lastResult);
			}
		}
	}

	public ProgressProvider Progress { get; private set; }

	public event Action? Started;

	public event Action? Stopped;

	public event Action? Succeeded;

	public event Action<Exception>? Failed;

	public event Action? Canceled;

	protected RedoableWorkerBase(Func<CancellationToken?, ProgressProvider?, TOut> workload, ProgressProvider? progress = null, [CallerMemberName] string creatorMemberName = "")
	{
		_003Cworkload_003EP = workload;
		_003CcreatorMemberName_003EP = creatorMemberName;
		Progress = progress ?? new ProgressProvider();
		idleEvent = new ManualResetEventSlim(initialState: true);
		base._002Ector();
	}

	public void Start(CancellationToken cancellationToken = default(CancellationToken), ProgressProvider? progressOverride = null)
	{
		lock (this)
		{
			lastToken = cancellationToken;
			if (progressOverride != null)
			{
				Progress = progressOverride;
			}
			if (running)
			{
				bool isEnabled;
				Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel> handler = new Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel>(7, 1, out isEnabled);
				if (isEnabled)
				{
					handler.AppendFormatted(_003CcreatorMemberName_003EP);
					handler.AppendLiteral("：接到重启请求");
				}
				Logger.Info(ref handler, LogBehavior.None, "E:\\Projects\\PCL\\本体代码\\MeloongCore\\Shared\\General\\Worker\\RedoableWorker.cs");
				pendingRedo = true;
				realCts?.Cancel();
				return;
			}
			running = true;
			idleEvent.Reset();
			realCts = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1] { lastToken });
		}
		Thread thread = new Thread(_Start);
		thread.IsBackground = true;
		thread.Name = "W/" + _003CcreatorMemberName_003EP;
		thread.Start();
	}

	private void _Start()
	{
		bool isEnabled;
		Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel> handler = new Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel>(5, 1, out isEnabled);
		if (isEnabled)
		{
			handler.AppendFormatted(_003CcreatorMemberName_003EP);
			handler.AppendLiteral("：运行开始");
		}
		Logger.Info(ref handler, LogBehavior.None, "E:\\Projects\\PCL\\本体代码\\MeloongCore\\Shared\\General\\Worker\\RedoableWorker.cs");
		this.Started?.Invoke();
		while (true)
		{
			try
			{
				realCts.Token.ThrowIfCancellationRequested();
				Progress?.Reset();
				TOut val = _003Cworkload_003EP(realCts.Token, Progress);
				realCts.Token.ThrowIfCancellationRequested();
				lock (this)
				{
					realCts?.Dispose();
					if (pendingRedo)
					{
						pendingRedo = false;
						realCts = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1] { lastToken });
						continue;
					}
					realCts = null;
					lastSucceeded = true;
					lastResult = val;
					hasSucceeded = true;
					running = false;
					idleEvent.Set();
					Progress?.Finish();
				}
				Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel> handler2 = new Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel>(5, 1, out isEnabled);
				if (isEnabled)
				{
					handler2.AppendFormatted(_003CcreatorMemberName_003EP);
					handler2.AppendLiteral("：运行成功");
				}
				Logger.Info(ref handler2, LogBehavior.None, "E:\\Projects\\PCL\\本体代码\\MeloongCore\\Shared\\General\\Worker\\RedoableWorker.cs");
				this.Succeeded?.Invoke();
				break;
			}
			catch (Exception ex)
			{
				if (!ex.IsCanceled())
				{
					Logger.Log(ex, _003CcreatorMemberName_003EP + "：运行失败" + (pendingRedo ? "，但即将重启，或可忽略" : ""), pendingRedo ? LogLevel.Warn : LogLevel.Error, LogBehavior.ToastIfDebug, "E:\\Projects\\PCL\\本体代码\\MeloongCore\\Shared\\General\\Worker\\RedoableWorker.cs");
				}
				lock (this)
				{
					realCts?.Dispose();
					if (pendingRedo)
					{
						bool isEnabled2;
						Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel> handler3 = new Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel>(5, 1, out isEnabled2);
						if (isEnabled2)
						{
							handler3.AppendFormatted(_003CcreatorMemberName_003EP);
							handler3.AppendLiteral("：重启开始");
						}
						Logger.Info(ref handler3, LogBehavior.None, "E:\\Projects\\PCL\\本体代码\\MeloongCore\\Shared\\General\\Worker\\RedoableWorker.cs");
						pendingRedo = false;
						realCts = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1] { lastToken });
						continue;
					}
					realCts = null;
					lastSucceeded = false;
					running = false;
					idleEvent.Set();
					Progress?.Skip();
				}
				if (ex.IsCanceled())
				{
					Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel> handler4 = new Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel>(6, 1, out isEnabled);
					if (isEnabled)
					{
						handler4.AppendFormatted(_003CcreatorMemberName_003EP);
						handler4.AppendLiteral("：运行已取消");
					}
					Logger.Info(ref handler4, LogBehavior.None, "E:\\Projects\\PCL\\本体代码\\MeloongCore\\Shared\\General\\Worker\\RedoableWorker.cs");
					this.Canceled?.Invoke();
				}
				else
				{
					this.Failed?.Invoke(ex);
				}
				break;
			}
		}
		this.Stopped?.Invoke();
	}

	public void Cancel()
	{
		lock (this)
		{
			pendingRedo = false;
			bool isEnabled;
			Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel> handler = new Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel>(7, 1, out isEnabled);
			if (isEnabled)
			{
				handler.AppendFormatted(_003CcreatorMemberName_003EP);
				handler.AppendLiteral("：接到取消请求");
			}
			Logger.Info(ref handler, LogBehavior.None, "E:\\Projects\\PCL\\本体代码\\MeloongCore\\Shared\\General\\Worker\\RedoableWorker.cs");
			realCts?.Cancel();
		}
	}

	public bool WaitIfRunning(int millisecondsTimeout = -1, CancellationToken cancellationToken = default(CancellationToken))
	{
		return idleEvent.Wait(millisecondsTimeout, cancellationToken);
	}

	public async Task<bool> WaitIfRunningAsync(int millisecondsTimeout = -1, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		RegisteredWaitHandle waitHandle = ThreadPool.RegisterWaitForSingleObject(idleEvent.WaitHandle, delegate(object _, bool timedOut)
		{
			completionSource.TrySetResult(!timedOut);
		}, null, millisecondsTimeout, executeOnlyOnce: true);
		using (cancellationToken.Register(delegate
		{
			completionSource.TrySetCanceled(cancellationToken);
		}))
		{
			try
			{
				return await completionSource.Task.NoCapture();
			}
			finally
			{
				waitHandle.Unregister(null);
			}
		}
	}
}
