using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CacheCow.Common.Http;

public class NotModifiedResponse : HttpResponseMessage
{
	public NotModifiedResponse(HttpRequestMessage request, CacheControlHeaderValue cacheControlHeaderValue)
		: this(request, cacheControlHeaderValue, null)
	{
	}

	public NotModifiedResponse(HttpRequestMessage request, CacheControlHeaderValue cacheControlHeaderValue, EntityTagHeaderValue etag)
		: base(HttpStatusCode.NotModified)
	{
		if (etag != null)
		{
			base.Headers.ETag = etag;
		}
		base.Headers.CacheControl = cacheControlHeaderValue;
		base.RequestMessage = request;
	}
}
