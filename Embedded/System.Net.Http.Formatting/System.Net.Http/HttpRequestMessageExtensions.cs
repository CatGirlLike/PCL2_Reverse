using System.ComponentModel;
using System.Web.Http;

namespace System.Net.Http;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class HttpRequestMessageExtensions
{
	public static HttpResponseMessage CreateResponse(this HttpRequestMessage request, HttpStatusCode statusCode)
	{
		if (request == null)
		{
			throw Error.ArgumentNull("request");
		}
		return new HttpResponseMessage
		{
			StatusCode = statusCode,
			RequestMessage = request
		};
	}

	public static HttpResponseMessage CreateResponse(this HttpRequestMessage request)
	{
		if (request == null)
		{
			throw Error.ArgumentNull("request");
		}
		return new HttpResponseMessage
		{
			RequestMessage = request
		};
	}
}
