using System;
using MeloongCore.Extensions;
using Microsoft.Win32;

namespace MeloongCore;

public static class RegistryUtils
{
	public const string NULL_STRING = "__NULL__";

	public static (object? result, bool exists) Read(string folder, string entry)
	{
		RegistryKey registryKey = GetRegistryKey(folder);
		try
		{
			object obj = ((registryKey != null) ? registryKey.GetValue(entry) : null);
			if (obj == null)
			{
				return (result: null, exists: false);
			}
			return (result: (obj is string text && text == "__NULL__") ? null : obj, exists: true);
		}
		finally
		{
			((IDisposable)registryKey)?.Dispose();
		}
	}

	public static object? TryRead(string folder, string entry, object? defaultValue = null)
	{
		try
		{
			(object? result, bool exists) tuple = Read(folder, entry);
			var (obj, _) = tuple;
			return tuple.exists ? obj : defaultValue;
		}
		catch (Exception ex)
		{
			Logger.Warn(ex, "读取注册表值失败（" + folder + "，" + entry + "）", LogBehavior.ToastIfDebug, "E:\\Projects\\PCL\\本体代码\\MeloongCore\\Shared\\Utils\\RegistryUtils.cs");
			return defaultValue;
		}
	}

	public static void Write(string folder, string entry, object? value)
	{
		RegistryKey registryKey = GetRegistryKey(folder, writable: false, create: true);
		try
		{
			registryKey.SetValue(entry, value ?? "__NULL__");
		}
		finally
		{
			((IDisposable)registryKey)?.Dispose();
		}
	}

	public static bool Has(string folder, string entry)
	{
		RegistryKey registryKey = GetRegistryKey(folder);
		try
		{
			if (registryKey == null)
			{
				return false;
			}
			object obj = new object();
			return registryKey.GetValue(entry, obj) != obj;
		}
		finally
		{
			((IDisposable)registryKey)?.Dispose();
		}
	}

	public static void Delete(string folder, string entry)
	{
		RegistryKey registryKey = GetRegistryKey(folder, writable: true);
		try
		{
			if (registryKey != null)
			{
				registryKey.DeleteValue(entry, false);
			}
		}
		finally
		{
			((IDisposable)registryKey)?.Dispose();
		}
	}

	private static RegistryKey? GetRegistryKey(string fullPath, bool writable = false, bool create = false)
	{
		RegistryKey val = (RegistryKey)(fullPath.BeforeFirst("\\").ToUpperInvariant() switch
		{
			"HKEY_CLASSES_ROOT" => Registry.ClassesRoot, 
			"HKEY_CURRENT_USER" => Registry.CurrentUser, 
			"HKEY_LOCAL_MACHINE" => Registry.LocalMachine, 
			"HKEY_USERS" => Registry.Users, 
			"HKEY_CURRENT_CONFIG" => Registry.CurrentConfig, 
			_ => throw new ArgumentException("注册表路径有误（" + fullPath + "）", "fullPath"), 
		});
		try
		{
			string text = fullPath.AfterFirst("\\");
			return create ? val.CreateSubKey(text) : val.OpenSubKey(text, writable);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
