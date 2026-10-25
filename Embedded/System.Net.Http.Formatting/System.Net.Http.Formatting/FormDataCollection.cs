using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http.Formatting.Internal;
using System.Net.Http.Formatting.Parsers;
using System.Net.Http.Properties;
using System.Text;
using System.Threading;
using System.Web.Http;

namespace System.Net.Http.Formatting;

public class FormDataCollection : IEnumerable<KeyValuePair<string, string>>, IEnumerable
{
	private readonly IEnumerable<KeyValuePair<string, string>> _pairs;

	private NameValueCollection _nameValueCollection;

	public string this[string name] => Get(name);

	public FormDataCollection(IEnumerable<KeyValuePair<string, string>> pairs)
	{
		if (pairs == null)
		{
			throw Error.ArgumentNull("pairs");
		}
		_pairs = pairs;
	}

	public FormDataCollection(Uri uri)
	{
		if (uri == null)
		{
			throw Error.ArgumentNull("uri");
		}
		string text = uri.Query;
		if (text != null && text.Length > 0 && text[0] == '?')
		{
			text = text.Substring(1);
		}
		_pairs = ParseQueryString(text);
	}

	public FormDataCollection(string query)
	{
		_pairs = ParseQueryString(query);
	}

	private static IEnumerable<KeyValuePair<string, string>> ParseQueryString(string query)
	{
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		if (string.IsNullOrWhiteSpace(query))
		{
			return list;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(query);
		FormUrlEncodedParser formUrlEncodedParser = new FormUrlEncodedParser(list, long.MaxValue);
		int bytesConsumed = 0;
		if (formUrlEncodedParser.ParseBuffer(bytes, bytes.Length, ref bytesConsumed, isFinal: true) != ParserState.Done)
		{
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.FormUrlEncodedParseError, bytesConsumed);
		}
		return list;
	}

	public NameValueCollection ReadAsNameValueCollection()
	{
		if (_nameValueCollection == null)
		{
			HttpValueCollection value = HttpValueCollection.Create(this);
			Interlocked.Exchange(ref _nameValueCollection, value);
		}
		return _nameValueCollection;
	}

	public string Get(string key)
	{
		return ReadAsNameValueCollection().Get(key);
	}

	public string[] GetValues(string key)
	{
		return ReadAsNameValueCollection().GetValues(key);
	}

	public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
	{
		return _pairs.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_pairs).GetEnumerator();
	}
}
