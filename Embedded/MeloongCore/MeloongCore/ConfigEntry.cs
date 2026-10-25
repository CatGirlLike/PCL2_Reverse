using System;
using System.Runtime.CompilerServices;

namespace MeloongCore;

public class ConfigEntry<T>
{
	public delegate void RefAction(ref T? value);

	[CompilerGenerated]
	private string _003Ckey_003EP;

	[CompilerGenerated]
	private T? _003CdefaultValue_003EP;

	[CompilerGenerated]
	private bool _003Cencrypted_003EP;

	public IConfigProvider Provider { get; }

	public event Action<T?>? Changed;

	public event Action<PreviewGetEventArgs<T>, IConfigProvider>? PreviewGet;

	public ConfigEntry(string key, T? defaultValue, bool encrypted = false, IConfigProvider? provider = null)
	{
		_003Ckey_003EP = key;
		_003CdefaultValue_003EP = defaultValue;
		_003Cencrypted_003EP = encrypted;
		Provider = provider ?? ConfigProviders.Default;
		base._002Ector();
	}

	public void Edit(Func<T?, T?> editFunc)
	{
		Set(editFunc(Get()));
	}

	public void Edit(RefAction editAction)
	{
		T value = Get();
		editAction(ref value);
		Set(value);
	}

	public void Set(T? value)
	{
		Provider.Set(_003Ckey_003EP, value, _003Cencrypted_003EP);
		this.Changed?.Invoke(value);
	}

	public void Reset()
	{
		if (Provider.HasValue(_003Ckey_003EP))
		{
			Provider.Remove(_003Ckey_003EP);
			this.Changed?.Invoke(_003CdefaultValue_003EP);
		}
	}

	public T? Get()
	{
		PreviewGetEventArgs<T> previewGetEventArgs = new PreviewGetEventArgs<T>(Provider.Read(_003Ckey_003EP, _003CdefaultValue_003EP, _003Cencrypted_003EP));
		this.PreviewGet?.Invoke(previewGetEventArgs, Provider);
		return previewGetEventArgs.Value;
	}

	public bool HasValue()
	{
		return Provider.HasValue(_003Ckey_003EP);
	}
}
