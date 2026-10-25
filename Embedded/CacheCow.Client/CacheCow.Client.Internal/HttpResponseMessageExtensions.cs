using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using CacheCow.Client.Headers;

namespace CacheCow.Client.Internal;

internal static class HttpResponseMessageExtensions
{
	public static HttpResponseMessage AddCacheCowHeader(this HttpResponseMessage response, CacheCowHeader header)
	{
		CacheCowHeader cacheCowHeader = response.Headers.GetCacheCowHeader();
		if (cacheCowHeader != null)
		{
			TraceWriter.WriteLine("WARNING: Already had this header: {0} NOw setting this: {1}", TraceLevel.Warning, cacheCowHeader, header);
			response.Headers.Remove("x-cachecow-client");
		}
		response.Headers.Add("x-cachecow-client", header.ToString());
		return response;
	}

	public static HttpResponseMessage CopyOtherCacheCowHeaders(this HttpResponseMessage response, HttpResponseMessage other)
	{
		foreach (KeyValuePair<string, IEnumerable<string>> header in other.Headers)
		{
			if (header.Key.StartsWith("x-cachecow"))
			{
				if (response.Headers.Contains(header.Key))
				{
					response.Headers.Remove(header.Key);
				}
				response.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}
		}
		return response;
	}
}
