using System;

namespace MeloongCore;

public class PreviewGetEventArgs<T> : EventArgs
{
	public T? Value { get; set; }

	public PreviewGetEventArgs(T? value)
	{
		Value = value;
		base._002Ector();
	}
}
