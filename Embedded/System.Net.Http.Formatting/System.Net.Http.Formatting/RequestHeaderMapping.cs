using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http.Formatting;

public class RequestHeaderMapping : MediaTypeMapping
{
	public string HeaderName { get; private set; }

	public string HeaderValue { get; private set; }

	public StringComparison HeaderValueComparison { get; private set; }

	public bool IsValueSubstring { get; private set; }

	public RequestHeaderMapping(string headerName, string headerValue, StringComparison valueComparison, bool isValueSubstring, string mediaType)
		: base(mediaType)
	{
		Initialize(headerName, headerValue, valueComparison, isValueSubstring);
	}

	public RequestHeaderMapping(string headerName, string headerValue, StringComparison valueComparison, bool isValueSubstring, MediaTypeHeaderValue mediaType)
		: base(mediaType)
	{
		Initialize(headerName, headerValue, valueComparison, isValueSubstring);
	}

	public override double TryMatchMediaType(HttpRequestMessage request)
	{
		if (request == null)
		{
			throw Error.ArgumentNull("request");
		}
		return MatchHeaderValue(request, HeaderName, HeaderValue, HeaderValueComparison, IsValueSubstring);
	}

	private static double MatchHeaderValue(HttpRequestMessage request, string headerName, string headerValue, StringComparison valueComparison, bool isValueSubstring)
	{
		if (request.Headers.TryGetValues(headerName, out var values))
		{
			foreach (string item in values)
			{
				if (isValueSubstring)
				{
					if (item.IndexOf(headerValue, valueComparison) != -1)
					{
						return 1.0;
					}
				}
				else if (item.Equals(headerValue, valueComparison))
				{
					return 1.0;
				}
			}
		}
		return 0.0;
	}

	private void Initialize(string headerName, string headerValue, StringComparison valueComparison, bool isValueSubstring)
	{
		if (string.IsNullOrWhiteSpace(headerName))
		{
			throw Error.ArgumentNull("headerName");
		}
		if (string.IsNullOrWhiteSpace(headerValue))
		{
			throw Error.ArgumentNull("headerValue");
		}
		StringComparisonHelper.Validate(valueComparison, "valueComparison");
		HeaderName = headerName;
		HeaderValue = headerValue;
		HeaderValueComparison = valueComparison;
		IsValueSubstring = isValueSubstring;
	}
}
