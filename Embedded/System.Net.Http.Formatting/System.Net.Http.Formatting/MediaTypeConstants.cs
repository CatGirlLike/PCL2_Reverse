using System.Net.Http.Headers;

namespace System.Net.Http.Formatting;

internal static class MediaTypeConstants
{
	private static readonly MediaTypeHeaderValue _defaultApplicationXmlMediaType = new MediaTypeHeaderValue("application/xml");

	private static readonly MediaTypeHeaderValue _defaultTextXmlMediaType = new MediaTypeHeaderValue("text/xml");

	private static readonly MediaTypeHeaderValue _defaultApplicationJsonMediaType = new MediaTypeHeaderValue("application/json");

	private static readonly MediaTypeHeaderValue _defaultTextJsonMediaType = new MediaTypeHeaderValue("text/json");

	private static readonly MediaTypeHeaderValue _defaultApplicationOctetStreamMediaType = new MediaTypeHeaderValue("application/octet-stream");

	private static readonly MediaTypeHeaderValue _defaultApplicationFormUrlEncodedMediaType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

	private static readonly MediaTypeHeaderValue _defaultApplicationBsonMediaType = new MediaTypeHeaderValue("application/bson");

	public static MediaTypeHeaderValue ApplicationOctetStreamMediaType => _defaultApplicationOctetStreamMediaType.Clone();

	public static MediaTypeHeaderValue ApplicationXmlMediaType => _defaultApplicationXmlMediaType.Clone();

	public static MediaTypeHeaderValue ApplicationJsonMediaType => _defaultApplicationJsonMediaType.Clone();

	public static MediaTypeHeaderValue TextXmlMediaType => _defaultTextXmlMediaType.Clone();

	public static MediaTypeHeaderValue TextJsonMediaType => _defaultTextJsonMediaType.Clone();

	public static MediaTypeHeaderValue ApplicationFormUrlEncodedMediaType => _defaultApplicationFormUrlEncodedMediaType.Clone();

	public static MediaTypeHeaderValue ApplicationBsonMediaType => _defaultApplicationBsonMediaType.Clone();
}
