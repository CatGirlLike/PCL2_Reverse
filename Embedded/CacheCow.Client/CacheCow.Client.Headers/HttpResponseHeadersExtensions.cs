using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace CacheCow.Client.Headers;

public static class HttpResponseHeadersExtensions
{
	public static CacheCowHeader GetCacheCowHeader(this HttpResponseHeaders headers)
	{
		CacheCowHeader cacheCowHeader = null;
		if (headers == null)
		{
			return null;
		}
		KeyValuePair<string, IEnumerable<string>> keyValuePair = headers.FirstOrDefault((KeyValuePair<string, IEnumerable<string>> x) => x.Key == "x-cachecow-client");
		if (keyValuePair.Value != null && keyValuePair.Value.Count() > 0)
		{
			CacheCowHeader.TryParse(keyValuePair.Value.Last(), out cacheCowHeader);
		}
		return cacheCowHeader;
	}
}
