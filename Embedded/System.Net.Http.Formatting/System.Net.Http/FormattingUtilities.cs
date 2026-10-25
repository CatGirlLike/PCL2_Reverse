using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace System.Net.Http;

internal static class FormattingUtilities
{
	private static readonly string[] dateFormats = new string[16]
	{
		"ddd, d MMM yyyy H:m:s 'GMT'", "ddd, d MMM yyyy H:m:s", "d MMM yyyy H:m:s 'GMT'", "d MMM yyyy H:m:s", "ddd, d MMM yy H:m:s 'GMT'", "ddd, d MMM yy H:m:s", "d MMM yy H:m:s 'GMT'", "d MMM yy H:m:s", "dddd, d'-'MMM'-'yy H:m:s 'GMT'", "dddd, d'-'MMM'-'yy H:m:s",
		"ddd, d'-'MMM'-'yyyy H:m:s 'GMT'", "ddd MMM d H:m:s yyyy", "ddd, d MMM yyyy H:m:s zzz", "ddd, d MMM yyyy H:m:s", "d MMM yyyy H:m:s zzz", "d MMM yyyy H:m:s"
	};

	private const string NonTokenChars = "()<>@,;:\\\"/[]?={}";

	public const double Match = 1.0;

	public const double NoMatch = 0.0;

	public const int DefaultMaxDepth = 256;

	public const int DefaultMinDepth = 1;

	public const string HttpRequestedWithHeader = "x-requested-with";

	public const string HttpRequestedWithHeaderValue = "XMLHttpRequest";

	public const string HttpHostHeader = "Host";

	public const string HttpVersionToken = "HTTP";

	public static readonly Type HttpRequestMessageType = typeof(HttpRequestMessage);

	public static readonly Type HttpResponseMessageType = typeof(HttpResponseMessage);

	public static readonly Type HttpContentType = typeof(HttpContent);

	public static readonly Type DelegatingEnumerableGenericType = typeof(DelegatingEnumerable<>);

	public static readonly Type EnumerableInterfaceGenericType = typeof(IEnumerable<>);

	public static readonly Type QueryableInterfaceGenericType = typeof(IQueryable<>);

	public static readonly XsdDataContractExporter XsdDataContractExporter = new XsdDataContractExporter();

	public static bool IsJTokenType(Type type)
	{
		return typeof(JToken).IsAssignableFrom(type);
	}

	public static HttpContentHeaders CreateEmptyContentHeaders()
	{
		HttpContent httpContent = null;
		HttpContentHeaders httpContentHeaders = null;
		try
		{
			httpContent = new StringContent(string.Empty);
			httpContentHeaders = httpContent.Headers;
			httpContentHeaders.Clear();
			return httpContentHeaders;
		}
		finally
		{
			httpContent?.Dispose();
		}
	}

	public static XmlDictionaryReaderQuotas CreateDefaultReaderQuotas()
	{
		return new XmlDictionaryReaderQuotas
		{
			MaxArrayLength = int.MaxValue,
			MaxBytesPerRead = int.MaxValue,
			MaxDepth = 256,
			MaxNameTableCharCount = int.MaxValue,
			MaxStringContentLength = int.MaxValue
		};
	}

	public static string UnquoteToken(string token)
	{
		if (string.IsNullOrWhiteSpace(token))
		{
			return token;
		}
		if (token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1)
		{
			return token.Substring(1, token.Length - 2);
		}
		return token;
	}

	public static bool ValidateHeaderToken(string token)
	{
		if (token == null)
		{
			return false;
		}
		foreach (char c in token)
		{
			if (c < '!' || c > '~' || "()<>@,;:\\\"/[]?={}".IndexOf(c) != -1)
			{
				return false;
			}
		}
		return true;
	}

	public static string DateToString(DateTimeOffset dateTime)
	{
		return dateTime.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture);
	}

	public static bool TryParseDate(string input, out DateTimeOffset result)
	{
		return DateTimeOffset.TryParseExact(input, dateFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal, out result);
	}

	public static bool TryParseInt32(string value, out int result)
	{
		return int.TryParse(value, NumberStyles.None, NumberFormatInfo.InvariantInfo, out result);
	}
}
