using System.Net.Http.Headers;

namespace System.Net.Http;

internal class HttpUnsortedResponse
{
	public Version Version { get; set; }

	public HttpStatusCode StatusCode { get; set; }

	public string ReasonPhrase { get; set; }

	public HttpHeaders HttpHeaders { get; private set; }

	public HttpUnsortedResponse()
	{
		HttpHeaders = new HttpUnsortedHeaders();
	}
}
