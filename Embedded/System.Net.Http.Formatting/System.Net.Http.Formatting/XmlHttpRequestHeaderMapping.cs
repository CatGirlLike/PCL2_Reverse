using System.Linq;
using System.Web.Http;

namespace System.Net.Http.Formatting;

public class XmlHttpRequestHeaderMapping : RequestHeaderMapping
{
	public XmlHttpRequestHeaderMapping()
		: base("x-requested-with", "XMLHttpRequest", StringComparison.OrdinalIgnoreCase, isValueSubstring: true, MediaTypeConstants.ApplicationJsonMediaType)
	{
	}

	public override double TryMatchMediaType(HttpRequestMessage request)
	{
		if (request == null)
		{
			throw Error.ArgumentNull("request");
		}
		if (request.Headers.Accept.Count == 0 || (request.Headers.Accept.Count == 1 && request.Headers.Accept.First().MediaType.Equals("*/*", StringComparison.Ordinal)))
		{
			return base.TryMatchMediaType(request);
		}
		return 0.0;
	}
}
