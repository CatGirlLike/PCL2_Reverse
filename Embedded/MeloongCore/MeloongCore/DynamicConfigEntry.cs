using System;
using System.Runtime.CompilerServices;

namespace MeloongCore;

public class DynamicConfigEntry<T>
{
	public delegate void RefAction(ref T? value);

	[CompilerGenerated]
	private string _003Ckey_003EP;

	[CompilerGenerated]
	private T? _003CdefaultValue_003EP;

	[CompilerGenerated]
	private bool _003Cencrypted_003EP;

	public event Action<T?>? Changed;

	public event Action<PreviewGetEventArgs<T>, IConfigProvider>? PreviewGet;

	public DynamicConfigEntry(string key, T? defaultValue, bool encrypted = false)
	{
		_003Ckey_003EP = key;
		_003CdefaultValue_003EP = defaultValue;
		_003Cencrypted_003EP = encrypted;
		base._002Ector();
	}

	public void Edit(Func<T?, T?> editFunc, IConfigProvider provider)
	{
		Set(editFunc(Get(provider)), provider);
	}

	public void Edit(RefAction editAction, IConfigProvider provider)
	{
		T value = Get(provider);
		editAction(ref value);
		Set(value, provider);
	}

	public void Set(T? value, IConfigProvider provider)
	{
		provider.Set(_003Ckey_003EP, value, _003Cencrypted_003EP);
		this.Changed?.Invoke(value);
	}

	public void Reset(IConfigProvider provider)
	{
		if (provider.HasValue(_003Ckey_003EP))
		{
			provider.Remove(_003Ckey_003EP);
			this.Changed?.Invoke(_003CdefaultValue_003EP);
		}
	}

	public T? Get(IConfigProvider provider)
	{
		PreviewGetEventArgs<T> previewGetEventArgs = new PreviewGetEventArgs<T>(provider.Read(_003Ckey_003EP, _003CdefaultValue_003EP, _003Cencrypted_003EP));
		this.PreviewGet?.Invoke(previewGetEventArgs, provider);
		return previewGetEventArgs.Value;
	}

	public bool HasValue(IConfigProvider provider)
	{
		return provider.HasValue(_003Ckey_003EP);
	}
}
