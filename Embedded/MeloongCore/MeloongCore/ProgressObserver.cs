using System;

namespace MeloongCore;

public class ProgressObserver : RateLimitedWorker
{
	private readonly ProgressProvider provider;

	public ProgressObserver(ProgressProvider provider, Action<double> updateAction, double minimalIntervalMs = 70.0)
	{
		ProgressProvider provider2 = provider;
		Action<double> updateAction2 = updateAction;
		base._002Ector(delegate
		{
			Observe(provider2, updateAction2);
		}, minimalIntervalMs, RateLimitMode.ImmediateThenMerge);
		this.provider = provider2;
		provider2.ProgressChanged += StartObserver;
	}

	public override void Dispose()
	{
		provider.ProgressChanged -= StartObserver;
		base.Dispose();
	}

	private void StartObserver((double actual, double skiped) raw)
	{
		Start();
	}

	private static void Observe(ProgressProvider provider, Action<double> updateAction)
	{
		double increasing = provider.GetIncreasing();
		updateAction(increasing);
	}
}
