using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http.Formatting;

public abstract class MediaTypeMapping
{
	public MediaTypeHeaderValue MediaType { get; private set; }

	protected MediaTypeMapping(MediaTypeHeaderValue mediaType)
	{
		if (mediaType == null)
		{
			throw Error.ArgumentNull("mediaType");
		}
		MediaType = mediaType;
	}

	protected MediaTypeMapping(string mediaType)
	{
		if (string.IsNullOrWhiteSpace(mediaType))
		{
			throw Error.ArgumentNull("mediaType");
		}
		MediaType = new MediaTypeHeaderValue(mediaType);
	}

	public abstract double TryMatchMediaType(HttpRequestMessage request);
}
