using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Web.Http;

namespace System.Net.Http.Formatting;

public class QueryStringMapping : MediaTypeMapping
{
	private static readonly Type _queryStringMappingType = typeof(QueryStringMapping);

	public string QueryStringParameterName { get; private set; }

	public string QueryStringParameterValue { get; private set; }

	public QueryStringMapping(string queryStringParameterName, string queryStringParameterValue, string mediaType)
		: base(mediaType)
	{
		Initialize(queryStringParameterName, queryStringParameterValue);
	}

	public QueryStringMapping(string queryStringParameterName, string queryStringParameterValue, MediaTypeHeaderValue mediaType)
		: base(mediaType)
	{
		Initialize(queryStringParameterName, queryStringParameterValue);
	}

	public override double TryMatchMediaType(HttpRequestMessage request)
	{
		if (request == null)
		{
			throw Error.ArgumentNull("request");
		}
		NameValueCollection queryString = GetQueryString(request.RequestUri);
		if (!DoesQueryStringMatch(queryString))
		{
			return 0.0;
		}
		return 1.0;
	}

	private static NameValueCollection GetQueryString(Uri uri)
	{
		if (uri == null)
		{
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.NonNullUriRequiredForMediaTypeMapping, _queryStringMappingType.Name);
		}
		return new FormDataCollection(uri).ReadAsNameValueCollection();
	}

	private void Initialize(string queryStringParameterName, string queryStringParameterValue)
	{
		if (string.IsNullOrWhiteSpace(queryStringParameterName))
		{
			throw Error.ArgumentNull("queryStringParameterName");
		}
		if (string.IsNullOrWhiteSpace(queryStringParameterValue))
		{
			throw Error.ArgumentNull("queryStringParameterValue");
		}
		QueryStringParameterName = queryStringParameterName.Trim();
		QueryStringParameterValue = queryStringParameterValue.Trim();
	}

	private bool DoesQueryStringMatch(NameValueCollection queryString)
	{
		if (queryString != null)
		{
			string[] allKeys = queryString.AllKeys;
			foreach (string text in allKeys)
			{
				if (string.Equals(text, QueryStringParameterName, StringComparison.OrdinalIgnoreCase) && string.Equals(queryString[text], QueryStringParameterValue, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}
}
