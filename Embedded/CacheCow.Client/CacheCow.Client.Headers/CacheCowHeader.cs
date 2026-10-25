using System;
using System.Reflection;
using System.Text;

namespace CacheCow.Client.Headers;

public class CacheCowHeader
{
	public static class ExtensionNames
	{
		public const string WasStale = "was-stale";

		public const string DidNotExist = "did-not-exist";

		public const string NotCacheable = "not-cacheable";

		public const string CacheValidationApplied = "cache-validation-applied";

		public const string RetrievedFromCache = "retrieved-from-cache";
	}

	public const string Name = "x-cachecow-client";

	public bool? WasStale { get; set; }

	public bool? DidNotExist { get; set; }

	public bool? NotCacheable { get; set; }

	public bool? CacheValidationApplied { get; set; }

	public bool? RetrievedFromCache { get; set; }

	public string Version { get; private set; }

	public CacheCowHeader()
	{
		Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Version);
		AddToStringBuilder(stringBuilder, WasStale, "was-stale");
		AddToStringBuilder(stringBuilder, NotCacheable, "not-cacheable");
		AddToStringBuilder(stringBuilder, DidNotExist, "did-not-exist");
		AddToStringBuilder(stringBuilder, CacheValidationApplied, "cache-validation-applied");
		AddToStringBuilder(stringBuilder, RetrievedFromCache, "retrieved-from-cache");
		return stringBuilder.ToString();
	}

	private void AddToStringBuilder(StringBuilder sb, bool? property, string extensionName)
	{
		if (property.HasValue)
		{
			sb.Append(';');
			sb.Append(extensionName);
			sb.Append('=');
			sb.Append(property.Value.ToString().ToLower());
		}
	}

	public static bool TryParse(string value, out CacheCowHeader cacheCowHeader)
	{
		cacheCowHeader = null;
		if (value == null)
		{
			return false;
		}
		if (value == string.Empty)
		{
			return false;
		}
		cacheCowHeader = new CacheCowHeader();
		string[] array = value.Split(new string[1] { ";" }, StringSplitOptions.None);
		cacheCowHeader.Version = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			cacheCowHeader.WasStale = cacheCowHeader.WasStale ?? ParseNameValue(array[i], "was-stale");
			cacheCowHeader.CacheValidationApplied = cacheCowHeader.CacheValidationApplied ?? ParseNameValue(array[i], "cache-validation-applied");
			cacheCowHeader.NotCacheable = cacheCowHeader.NotCacheable ?? ParseNameValue(array[i], "not-cacheable");
			cacheCowHeader.DidNotExist = cacheCowHeader.DidNotExist ?? ParseNameValue(array[i], "did-not-exist");
			cacheCowHeader.RetrievedFromCache = cacheCowHeader.RetrievedFromCache ?? ParseNameValue(array[i], "retrieved-from-cache");
		}
		return true;
	}

	private static bool? ParseNameValue(string entry, string name)
	{
		if (string.IsNullOrEmpty(entry))
		{
			return null;
		}
		string[] array = entry.Split('=');
		if (array.Length != 2)
		{
			return null;
		}
		array[0] = array[0].Trim();
		array[1] = array[1].Trim();
		if (array[0].ToLower() != name)
		{
			return null;
		}
		bool result = false;
		if (!bool.TryParse(array[1], out result))
		{
			return null;
		}
		return result;
	}
}
