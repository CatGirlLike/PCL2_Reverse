using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http;

public class MultipartFileData
{
	public HttpContentHeaders Headers { get; private set; }

	public string LocalFileName { get; private set; }

	public MultipartFileData(HttpContentHeaders headers, string localFileName)
	{
		if (headers == null)
		{
			throw Error.ArgumentNull("headers");
		}
		if (localFileName == null)
		{
			throw Error.ArgumentNull("localFileName");
		}
		Headers = headers;
		LocalFileName = localFileName;
	}
}
