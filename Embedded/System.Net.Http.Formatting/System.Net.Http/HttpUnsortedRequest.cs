using System.Net.Http.Headers;

namespace System.Net.Http;

internal class HttpUnsortedRequest
{
	public HttpMethod Method { get; set; }

	public string RequestUri { get; set; }

	public Version Version { get; set; }

	public HttpHeaders HttpHeaders { get; private set; }

	public HttpUnsortedRequest()
	{
		HttpHeaders = new HttpUnsortedHeaders();
	}
}
