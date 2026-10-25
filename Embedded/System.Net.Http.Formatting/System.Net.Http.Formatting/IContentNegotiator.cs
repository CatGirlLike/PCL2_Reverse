using System.Collections.Generic;

namespace System.Net.Http.Formatting;

public interface IContentNegotiator
{
	ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters);
}
