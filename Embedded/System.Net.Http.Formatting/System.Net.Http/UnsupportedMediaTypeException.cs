using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http;

public class UnsupportedMediaTypeException : Exception
{
	public MediaTypeHeaderValue MediaType { get; private set; }

	public UnsupportedMediaTypeException(string message, MediaTypeHeaderValue mediaType)
		: base(message)
	{
		if (mediaType == null)
		{
			throw Error.ArgumentNull("mediaType");
		}
		MediaType = mediaType;
	}
}
