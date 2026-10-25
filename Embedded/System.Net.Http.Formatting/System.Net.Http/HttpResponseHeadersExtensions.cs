using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Web.Http;

namespace System.Net.Http;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class HttpResponseHeadersExtensions
{
	private const string SetCookie = "Set-Cookie";

	public static void AddCookies(this HttpResponseHeaders headers, IEnumerable<CookieHeaderValue> cookies)
	{
		if (headers == null)
		{
			throw Error.ArgumentNull("headers");
		}
		if (cookies == null)
		{
			throw Error.ArgumentNull("cookies");
		}
		foreach (CookieHeaderValue cookie in cookies)
		{
			if (cookie == null)
			{
				throw Error.Argument("cookies", System.Net.Http.Properties.Resources.CookieNull);
			}
			headers.TryAddWithoutValidation("Set-Cookie", cookie.ToString());
		}
	}
}
