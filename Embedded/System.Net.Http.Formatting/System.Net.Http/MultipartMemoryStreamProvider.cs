using System.IO;
using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http;

public class MultipartMemoryStreamProvider : MultipartStreamProvider
{
	public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
	{
		if (parent == null)
		{
			throw Error.ArgumentNull("parent");
		}
		if (headers == null)
		{
			throw Error.ArgumentNull("headers");
		}
		return new MemoryStream();
	}
}
