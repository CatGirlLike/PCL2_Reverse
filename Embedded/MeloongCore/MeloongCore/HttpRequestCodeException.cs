using System.Net;
using System.Net.Http;

namespace MeloongCore;

public class HttpRequestCodeException : HttpRequestException
{
	public new HttpStatusCode StatusCode { get; }

	public string? Response { get; }

	public HttpRequestCodeException(string message, HttpStatusCode statusCode, string? response)
	{
		StatusCode = statusCode;
		Response = response;
		base._002Ector(message);
	}
}
