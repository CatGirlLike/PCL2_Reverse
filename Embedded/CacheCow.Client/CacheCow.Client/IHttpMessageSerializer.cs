using System.IO;
using System.Net.Http;

namespace CacheCow.Client;

public interface IHttpMessageSerializer
{
	void Serialize(HttpResponseMessage response, Stream stream);

	void Serialize(HttpRequestMessage request, Stream stream);

	HttpResponseMessage DeserializeToResponse(Stream stream);

	HttpRequestMessage DeserializeToRequest(Stream stream);
}
