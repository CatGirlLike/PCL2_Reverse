using System.Net.Http;
using System.Net.Http.Headers;

namespace CacheCow.Client.Internal;

internal static class OtherExtensions
{
	public static bool ShouldRevalidate(this CacheControlHeaderValue headerValue, bool defaultBehaviour)
	{
		if (headerValue == null)
		{
			return false;
		}
		if (!defaultBehaviour && !headerValue.MustRevalidate)
		{
			return headerValue.NoCache;
		}
		return true;
	}

	public static bool IsPutPatchOrDelete(this HttpMethod method)
	{
		if (!(method == HttpMethod.Delete) && !(method == HttpMethod.Put))
		{
			return method.Method.ToUpper() == "PATCH";
		}
		return true;
	}

	public static bool IsCacheIgnorable(this HttpMethod method)
	{
		if (!method.IsPutPatchOrDelete())
		{
			return method != HttpMethod.Get;
		}
		return false;
	}
}
