using System.Net.Http;
using CacheCow.Common;

namespace CacheCow.Client;

public static class ClientExtensions
{
	public static HttpClient CreateClient(HttpMessageHandler handler = null)
	{
		return new HttpClient(new CachingHandler
		{
			InnerHandler = (handler ?? new HttpClientHandler())
		});
	}

	public static HttpClient CreateClient(this ICacheStore store, HttpMessageHandler handler = null)
	{
		return new HttpClient(new CachingHandler(store)
		{
			InnerHandler = (handler ?? new HttpClientHandler())
		});
	}
}
