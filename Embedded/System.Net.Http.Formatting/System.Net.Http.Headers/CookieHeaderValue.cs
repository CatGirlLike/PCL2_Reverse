using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http;

namespace System.Net.Http.Headers;

public class CookieHeaderValue : ICloneable
{
	private const string ExpiresToken = "expires";

	private const string MaxAgeToken = "max-age";

	private const string DomainToken = "domain";

	private const string PathToken = "path";

	private const string SecureToken = "secure";

	private const string HttpOnlyToken = "httponly";

	private const string DefaultPath = "/";

	private static readonly char[] segmentSeparator = new char[1] { ';' };

	private static readonly char[] nameValueSeparator = new char[1] { '=' };

	private Collection<CookieState> _cookies;

	public Collection<CookieState> Cookies
	{
		get
		{
			if (_cookies == null)
			{
				_cookies = new Collection<CookieState>();
			}
			return _cookies;
		}
	}

	public DateTimeOffset? Expires { get; set; }

	public TimeSpan? MaxAge { get; set; }

	public string Domain { get; set; }

	public string Path { get; set; }

	public bool Secure { get; set; }

	public bool HttpOnly { get; set; }

	public CookieState this[string name]
	{
		get
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}
			CookieState cookieState = Cookies.FirstOrDefault((CookieState c) => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
			if (cookieState == null)
			{
				cookieState = new CookieState(name, string.Empty);
				Cookies.Add(cookieState);
			}
			return cookieState;
		}
	}

	public CookieHeaderValue(string name, string value)
	{
		CookieState item = new CookieState(name, value);
		Cookies.Add(item);
	}

	public CookieHeaderValue(string name, NameValueCollection values)
	{
		CookieState item = new CookieState(name, values);
		Cookies.Add(item);
	}

	protected CookieHeaderValue()
	{
	}

	private CookieHeaderValue(CookieHeaderValue source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		Expires = source.Expires;
		MaxAge = source.MaxAge;
		Domain = source.Domain;
		Path = source.Path;
		Secure = source.Secure;
		HttpOnly = source.HttpOnly;
		foreach (CookieState cookie in source.Cookies)
		{
			Cookies.Add(CloneableExtensions.Clone(cookie));
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool first = true;
		foreach (CookieState cookie in Cookies)
		{
			first = AppendSegment(stringBuilder, first, cookie.ToString(), null);
		}
		if (Expires.HasValue)
		{
			first = AppendSegment(stringBuilder, first, "expires", FormattingUtilities.DateToString(Expires.Value));
		}
		if (MaxAge.HasValue)
		{
			first = AppendSegment(stringBuilder, first, "max-age", ((int)MaxAge.Value.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo));
		}
		if (Domain != null)
		{
			first = AppendSegment(stringBuilder, first, "domain", Domain);
		}
		if (Path != null)
		{
			first = AppendSegment(stringBuilder, first, "path", Path);
		}
		if (Secure)
		{
			first = AppendSegment(stringBuilder, first, "secure", null);
		}
		if (HttpOnly)
		{
			first = AppendSegment(stringBuilder, first, "httponly", null);
		}
		return stringBuilder.ToString();
	}

	public object Clone()
	{
		return new CookieHeaderValue(this);
	}

	public static bool TryParse(string input, out CookieHeaderValue parsedValue)
	{
		parsedValue = null;
		if (!string.IsNullOrEmpty(input))
		{
			string[] array = input.Split(segmentSeparator);
			CookieHeaderValue cookieHeaderValue = new CookieHeaderValue();
			string[] array2 = array;
			foreach (string segment in array2)
			{
				if (!ParseCookieSegment(cookieHeaderValue, segment))
				{
					return false;
				}
			}
			if (cookieHeaderValue.Cookies.Count == 0)
			{
				return false;
			}
			parsedValue = cookieHeaderValue;
			return true;
		}
		return false;
	}

	private static bool AppendSegment(StringBuilder builder, bool first, string name, string value)
	{
		if (first)
		{
			first = false;
		}
		else
		{
			builder.Append("; ");
		}
		builder.Append(name);
		if (value != null)
		{
			builder.Append("=");
			builder.Append(value);
		}
		return first;
	}

	private static bool ParseCookieSegment(CookieHeaderValue instance, string segment)
	{
		if (string.IsNullOrWhiteSpace(segment))
		{
			return true;
		}
		string[] array = segment.Split(nameValueSeparator, 2);
		if (array.Length < 1 || string.IsNullOrWhiteSpace(array[0]))
		{
			return false;
		}
		string text = array[0].Trim();
		if (string.Equals(text, "expires", StringComparison.OrdinalIgnoreCase))
		{
			if (FormattingUtilities.TryParseDate(GetSegmentValue(array, null), out var result))
			{
				instance.Expires = result;
				return true;
			}
			return false;
		}
		if (string.Equals(text, "max-age", StringComparison.OrdinalIgnoreCase))
		{
			if (FormattingUtilities.TryParseInt32(GetSegmentValue(array, null), out var result2))
			{
				instance.MaxAge = new TimeSpan(0, 0, result2);
				return true;
			}
			return false;
		}
		if (string.Equals(text, "domain", StringComparison.OrdinalIgnoreCase))
		{
			instance.Domain = GetSegmentValue(array, null);
			return true;
		}
		if (string.Equals(text, "path", StringComparison.OrdinalIgnoreCase))
		{
			instance.Path = GetSegmentValue(array, "/");
			return true;
		}
		if (string.Equals(text, "secure", StringComparison.OrdinalIgnoreCase))
		{
			if (!string.IsNullOrWhiteSpace(GetSegmentValue(array, null)))
			{
				return false;
			}
			instance.Secure = true;
			return true;
		}
		if (string.Equals(text, "httponly", StringComparison.OrdinalIgnoreCase))
		{
			if (!string.IsNullOrWhiteSpace(GetSegmentValue(array, null)))
			{
				return false;
			}
			instance.HttpOnly = true;
			return true;
		}
		string segmentValue = GetSegmentValue(array, null);
		try
		{
			NameValueCollection values = new FormDataCollection(segmentValue).ReadAsNameValueCollection();
			CookieState item = new CookieState(text, values);
			instance.Cookies.Add(item);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static string GetSegmentValue(string[] nameValuePair, string defaultValue)
	{
		if (nameValuePair.Length <= 1)
		{
			return defaultValue;
		}
		return FormattingUtilities.UnquoteToken(nameValuePair[1]);
	}
}
