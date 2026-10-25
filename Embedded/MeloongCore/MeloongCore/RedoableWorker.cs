using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MeloongCore;

public class RedoableWorker<TOut> : RedoableWorkerBase<TOut>
{
	public RedoableWorker(Func<CancellationToken?, ProgressProvider?, TOut> workload, [CallerMemberName] string creatorMemberName = "")
		: base(workload, (ProgressProvider?)null, creatorMemberName)
	{
	}

	public RedoableWorker(Func<CancellationToken?, TOut> workload, [CallerMemberName] string creatorMemberName = "")
	{
		Func<CancellationToken?, TOut> workload2 = workload;
		base._002Ector((Func<CancellationToken?, ProgressProvider?, TOut>)((CancellationToken? c, ProgressProvider? _) => workload2(c)), (ProgressProvider?)null, creatorMemberName);
	}

	public RedoableWorker(Func<TOut> workload, [CallerMemberName] string creatorMemberName = "")
	{
		Func<TOut> workload2 = workload;
		base._002Ector((Func<CancellationToken?, ProgressProvider?, TOut>)((CancellationToken? _, ProgressProvider? _) => workload2()), (ProgressProvider?)null, creatorMemberName);
	}
}
public class RedoableWorker : RedoableWorkerBase<NoType?>
{
	public RedoableWorker(Action<CancellationToken?, ProgressProvider?> workload, [CallerMemberName] string creatorMemberName = "")
	{
		Action<CancellationToken?, ProgressProvider?> workload2 = workload;
		base._002Ector((Func<CancellationToken?, ProgressProvider?, NoType>)delegate(CancellationToken? c, ProgressProvider? p)
		{
			workload2(c, p);
			return (NoType?)null;
		}, (ProgressProvider?)null, creatorMemberName);
	}

	public RedoableWorker(Action<CancellationToken?> workload, [CallerMemberName] string creatorMemberName = "")
	{
		Action<CancellationToken?> workload2 = workload;
		base._002Ector((Func<CancellationToken?, ProgressProvider?, NoType>)delegate(CancellationToken? c, ProgressProvider? _)
		{
			workload2(c);
			return (NoType?)null;
		}, (ProgressProvider?)null, creatorMemberName);
	}

	public RedoableWorker(Action workload, [CallerMemberName] string creatorMemberName = "")
	{
		Action workload2 = workload;
		base._002Ector((Func<CancellationToken?, ProgressProvider?, NoType>)delegate
		{
			workload2();
			return (NoType?)null;
		}, (ProgressProvider?)null, creatorMemberName);
	}
}
