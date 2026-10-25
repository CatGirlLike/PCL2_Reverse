using System.Reflection;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace System.Net.Http.Formatting;

public class JsonContractResolver : DefaultContractResolver
{
	private readonly MediaTypeFormatter _formatter;

	public JsonContractResolver(MediaTypeFormatter formatter)
	{
		if (formatter == null)
		{
			throw Error.ArgumentNull("formatter");
		}
		_formatter = formatter;
		base.IgnoreSerializableAttribute = false;
	}

	private void ConfigureProperty(MemberInfo member, JsonProperty property)
	{
		if (_formatter.RequiredMemberSelector != null && _formatter.RequiredMemberSelector.IsRequiredMember(member))
		{
			property.Required = Required.AllowNull;
			property.DefaultValueHandling = DefaultValueHandling.Include;
			property.NullValueHandling = NullValueHandling.Include;
		}
		else
		{
			property.Required = Required.Default;
		}
	}

	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
	{
		JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);
		ConfigureProperty(member, jsonProperty);
		return jsonProperty;
	}
}
