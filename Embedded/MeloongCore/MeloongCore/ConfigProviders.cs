using System.IO;

namespace MeloongCore;

public static class ConfigProviders
{
	public static JsonConfigProvider Default = new JsonConfigProvider(Path.Combine(Paths.AppDataThenName, "config.json"));
}
