using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http;

public class MultipartRemoteFileData
{
	public string FileName { get; private set; }

	public HttpContentHeaders Headers { get; private set; }

	public string Location { get; private set; }

	public MultipartRemoteFileData(HttpContentHeaders headers, string location, string fileName)
	{
		if (headers == null)
		{
			throw Error.ArgumentNull("headers");
		}
		if (location == null)
		{
			throw Error.ArgumentNull("location");
		}
		if (fileName == null)
		{
			throw Error.ArgumentNull("fileName");
		}
		FileName = fileName;
		Headers = headers;
		Location = location;
	}
}
