using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Properties;
using System.Web.Http;

namespace System.Net.Http;

public static class HttpClientFactory
{
	public static HttpClient Create(params DelegatingHandler[] handlers)
	{
		return Create(new HttpClientHandler(), handlers);
	}

	public static HttpClient Create(HttpMessageHandler innerHandler, params DelegatingHandler[] handlers)
	{
		return new HttpClient(CreatePipeline(innerHandler, handlers));
	}

	public static HttpMessageHandler CreatePipeline(HttpMessageHandler innerHandler, IEnumerable<DelegatingHandler> handlers)
	{
		if (innerHandler == null)
		{
			throw Error.ArgumentNull("innerHandler");
		}
		if (handlers == null)
		{
			return innerHandler;
		}
		HttpMessageHandler httpMessageHandler = innerHandler;
		foreach (DelegatingHandler item in handlers.Reverse())
		{
			if (item == null)
			{
				throw Error.Argument("handlers", System.Net.Http.Properties.Resources.DelegatingHandlerArrayContainsNullItem, typeof(DelegatingHandler).Name);
			}
			if (item.InnerHandler != null)
			{
				throw Error.Argument("handlers", System.Net.Http.Properties.Resources.DelegatingHandlerArrayHasNonNullInnerHandler, typeof(DelegatingHandler).Name, "InnerHandler", item.GetType().Name);
			}
			item.InnerHandler = httpMessageHandler;
			httpMessageHandler = item;
		}
		return httpMessageHandler;
	}
}
