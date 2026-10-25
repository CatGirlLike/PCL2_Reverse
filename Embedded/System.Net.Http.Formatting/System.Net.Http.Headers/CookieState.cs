using System.Collections.Specialized;
using System.Net.Http.Formatting.Internal;
using System.Net.Http.Properties;
using System.Web.Http;

namespace System.Net.Http.Headers;

public class CookieState : ICloneable
{
	private string _name;

	private NameValueCollection _values = HttpValueCollection.Create();

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			CheckNameFormat(value, "value");
			_name = value;
		}
	}

	public string Value
	{
		get
		{
			if (Values.Count <= 0)
			{
				return string.Empty;
			}
			return Values.AllKeys[0];
		}
		set
		{
			CheckValueFormat(value, "value");
			if (Values.Count > 0)
			{
				Values.AllKeys[0] = value;
			}
			else
			{
				Values.Add(value, string.Empty);
			}
		}
	}

	public NameValueCollection Values => _values;

	public string this[string subName]
	{
		get
		{
			return Values[subName];
		}
		set
		{
			Values[subName] = value;
		}
	}

	public CookieState(string name)
		: this(name, string.Empty)
	{
	}

	public CookieState(string name, string value)
	{
		CheckNameFormat(name, "name");
		_name = name;
		CheckValueFormat(value, "value");
		Value = value;
	}

	public CookieState(string name, NameValueCollection values)
	{
		CheckNameFormat(name, "name");
		_name = name;
		if (values == null)
		{
			throw Error.ArgumentNull("values");
		}
		Values.Add(values);
	}

	private CookieState(CookieState source)
	{
		_name = source._name;
		if (source._values != null)
		{
			Values.Add(source._values);
		}
	}

	public override string ToString()
	{
		return _name + "=" + ((_values != null) ? _values.ToString() : string.Empty);
	}

	public object Clone()
	{
		return new CookieState(this);
	}

	private static void CheckNameFormat(string name, string parameterName)
	{
		if (name == null)
		{
			throw Error.ArgumentNull("name");
		}
		if (!FormattingUtilities.ValidateHeaderToken(name))
		{
			throw Error.Argument(parameterName, System.Net.Http.Properties.Resources.CookieInvalidName);
		}
	}

	private static void CheckValueFormat(string value, string parameterName)
	{
		if (value == null)
		{
			throw Error.ArgumentNull(parameterName);
		}
	}
}
