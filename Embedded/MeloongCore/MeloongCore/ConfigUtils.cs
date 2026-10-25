using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace MeloongCore;

public static class ConfigUtils
{
	private static readonly Lazy<JObject> secret = new Lazy<JObject>(delegate
	{
		string text = Path.Combine(Paths.AppData, "Meloong", "Dev", "secret.json");
		try
		{
			return FileUtils.Exists(text) ? JObject.Parse(FileUtils.ReadAsString(text)) : new JObject();
		}
		catch
		{
			throw new InvalidDataException("读取密钥文件失败（" + text + "），请检查文件内容与加密格式。");
		}
	});

	public static void SaveAll()
	{
		foreach (WeakReference<JsonConfigProvider> allProvider in JsonConfigProvider.allProviders)
		{
			if (allProvider.TryGetTarget(out var target))
			{
				target.Save();
			}
		}
	}

	public static string GetSecret(string key)
	{
		string encryptedString = secret.Value.Value<string>(key) ?? throw new KeyNotFoundException("未找到密钥：" + key);
		try
		{
			return CryptographyUtils.AesDecrypt(encryptedString);
		}
		catch
		{
			throw new InvalidDataException("读取密钥失败（" + key + "），请检查文件内容与加密格式。");
		}
	}
}
