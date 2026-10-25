using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http.Properties;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Http;

namespace System.Net.Http.Formatting.Internal;

[Serializable]
internal class HttpValueCollection : NameValueCollection
{
	protected HttpValueCollection(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private HttpValueCollection()
		: base(StringComparer.OrdinalIgnoreCase)
	{
	}

	internal static HttpValueCollection Create()
	{
		return new HttpValueCollection();
	}

	internal static HttpValueCollection Create(IEnumerable<KeyValuePair<string, string>> pairs)
	{
		HttpValueCollection httpValueCollection = new HttpValueCollection();
		foreach (KeyValuePair<string, string> pair in pairs)
		{
			httpValueCollection.Add(pair.Key, pair.Value);
		}
		httpValueCollection.IsReadOnly = false;
		return httpValueCollection;
	}

	public override void Add(string name, string value)
	{
		ThrowIfMaxHttpCollectionKeysExceeded(Count);
		name = name ?? string.Empty;
		value = value ?? string.Empty;
		base.Add(name, value);
	}

	public override string ToString()
	{
		return ToString(urlEncode: true);
	}

	private static void ThrowIfMaxHttpCollectionKeysExceeded(int count)
	{
		if (count >= MediaTypeFormatter.MaxHttpCollectionKeys)
		{
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.MaxHttpCollectionKeyLimitReached, MediaTypeFormatter.MaxHttpCollectionKeys, typeof(MediaTypeFormatter));
		}
	}

	private string ToString(bool urlEncode)
	{
		if (Count == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool first = true;
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				string name = (string)enumerator.Current;
				string[] values = GetValues(name);
				if (values == null || values.Length == 0)
				{
					first = AppendNameValuePair(stringBuilder, first, urlEncode, name, string.Empty);
					continue;
				}
				string[] array = values;
				foreach (string value in array)
				{
					first = AppendNameValuePair(stringBuilder, first, urlEncode, name, value);
				}
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		return stringBuilder.ToString();
	}

	private static bool AppendNameValuePair(StringBuilder builder, bool first, bool urlEncode, string name, string value)
	{
		string text = name ?? string.Empty;
		string value2 = (urlEncode ? UriQueryUtility.UrlEncode(text) : text);
		string text2 = value ?? string.Empty;
		string value3 = (urlEncode ? UriQueryUtility.UrlEncode(text2) : text2);
		if (first)
		{
			first = false;
		}
		else
		{
			builder.Append("&");
		}
		builder.Append(value2);
		if (!string.IsNullOrEmpty(value3))
		{
			builder.Append("=");
			builder.Append(value3);
		}
		return first;
	}
}
