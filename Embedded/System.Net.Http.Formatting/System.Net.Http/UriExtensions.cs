using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http.Formatting;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace System.Net.Http;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class UriExtensions
{
	public static NameValueCollection ParseQueryString(this Uri address)
	{
		if (address == null)
		{
			throw Error.ArgumentNull("address");
		}
		return new FormDataCollection(address).ReadAsNameValueCollection();
	}

	public static bool TryReadQueryAsJson(this Uri address, out JObject value)
	{
		if (address == null)
		{
			throw Error.ArgumentNull("address");
		}
		return FormUrlEncodedJson.TryParse(new FormDataCollection(address), out value);
	}

	public static bool TryReadQueryAs(this Uri address, Type type, out object value)
	{
		if (address == null)
		{
			throw Error.ArgumentNull("address");
		}
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (FormUrlEncodedJson.TryParse(new FormDataCollection(address), out var value2))
		{
			using (JTokenReader reader = new JTokenReader(value2))
			{
				value = new JsonSerializer().Deserialize(reader, type);
			}
			return true;
		}
		value = null;
		return false;
	}

	public static bool TryReadQueryAs<T>(this Uri address, out T value)
	{
		if (address == null)
		{
			throw Error.ArgumentNull("address");
		}
		if (FormUrlEncodedJson.TryParse(new FormDataCollection(address), out var value2))
		{
			value = value2.ToObject<T>();
			return true;
		}
		value = default(T);
		return false;
	}
}
