using System.ComponentModel;
using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http.Formatting;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class MediaTypeFormatterExtensions
{
	public static void AddQueryStringMapping(this MediaTypeFormatter formatter, string queryStringParameterName, string queryStringParameterValue, MediaTypeHeaderValue mediaType)
	{
		if (formatter == null)
		{
			throw Error.ArgumentNull("formatter");
		}
		QueryStringMapping item = new QueryStringMapping(queryStringParameterName, queryStringParameterValue, mediaType);
		formatter.MediaTypeMappings.Add(item);
	}

	public static void AddQueryStringMapping(this MediaTypeFormatter formatter, string queryStringParameterName, string queryStringParameterValue, string mediaType)
	{
		if (formatter == null)
		{
			throw Error.ArgumentNull("formatter");
		}
		QueryStringMapping item = new QueryStringMapping(queryStringParameterName, queryStringParameterValue, mediaType);
		formatter.MediaTypeMappings.Add(item);
	}

	public static void AddRequestHeaderMapping(this MediaTypeFormatter formatter, string headerName, string headerValue, StringComparison valueComparison, bool isValueSubstring, MediaTypeHeaderValue mediaType)
	{
		if (formatter == null)
		{
			throw Error.ArgumentNull("formatter");
		}
		RequestHeaderMapping item = new RequestHeaderMapping(headerName, headerValue, valueComparison, isValueSubstring, mediaType);
		formatter.MediaTypeMappings.Add(item);
	}

	public static void AddRequestHeaderMapping(this MediaTypeFormatter formatter, string headerName, string headerValue, StringComparison valueComparison, bool isValueSubstring, string mediaType)
	{
		if (formatter == null)
		{
			throw Error.ArgumentNull("formatter");
		}
		RequestHeaderMapping item = new RequestHeaderMapping(headerName, headerValue, valueComparison, isValueSubstring, mediaType);
		formatter.MediaTypeMappings.Add(item);
	}
}
