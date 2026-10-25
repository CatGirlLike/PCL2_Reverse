using System.Configuration;

namespace CacheCow.Common;

public class ConfigurationValueProvider : IConfigurationValueProvider
{
	public string GetValue(string key)
	{
		return ConfigurationManager.AppSettings[key];
	}
}
