using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class HttpRequestHeadersExtensions
{
	private const string Cookie = "Cookie";

	public static Collection<CookieHeaderValue> GetCookies(this HttpRequestHeaders headers)
	{
		if (headers == null)
		{
			throw Error.ArgumentNull("headers");
		}
		Collection<CookieHeaderValue> collection = new Collection<CookieHeaderValue>();
		if (headers.TryGetValues("Cookie", out var values))
		{
			foreach (string item in values)
			{
				if (CookieHeaderValue.TryParse(item, out var parsedValue))
				{
					collection.Add(parsedValue);
				}
			}
		}
		return collection;
	}

	public static Collection<CookieHeaderValue> GetCookies(this HttpRequestHeaders headers, string name)
	{
		if (name == null)
		{
			throw Error.ArgumentNull("name");
		}
		return new Collection<CookieHeaderValue>((from header in headers.GetCookies()
			where header.Cookies.Any((CookieState state) => string.Equals(state.Name, name, StringComparison.OrdinalIgnoreCase))
			select header).ToArray());
	}
}
