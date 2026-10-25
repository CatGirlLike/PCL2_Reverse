using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace CacheCow.Common.Helpers;

public static class HttpHeadersExtensions
{
	public static void Parse(this HttpHeaders httpHeaders, string headers)
	{
		if (httpHeaders == null)
		{
			throw new ArgumentNullException("httpHeaders");
		}
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		string text = null;
		string text2 = null;
		string[] array = headers.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string obj in array)
		{
			int num = obj.IndexOf(":");
			text = obj.Substring(0, num);
			text2 = obj.Substring(num + 1).Trim();
			if (!httpHeaders.TryAddWithoutValidation(text, text2))
			{
				throw new InvalidOperationException($"Value {text2} for header {text} not acceptable.");
			}
		}
	}

	public static IEnumerable<KeyValuePair<string, IEnumerable<string>>> ExtractHeadersValues(this HttpRequestHeaders headers, params string[] headerNames)
	{
		return headers.Where((KeyValuePair<string, IEnumerable<string>> x) => headerNames.Any((string h) => h.Equals(x.Key, StringComparison.CurrentCultureIgnoreCase)));
	}
}
