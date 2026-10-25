namespace CacheCow.Common;

public interface IConfigurationValueProvider
{
	string GetValue(string key);
}
