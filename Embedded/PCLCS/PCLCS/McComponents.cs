using System.Collections.Generic;
using System.Linq;
using MeloongCore.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PCLCS;

public record McComponents(string MinecraftVersion, Dictionary<ModLoaders, string> ModLoaderVersions, [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] JObject? OptiFineEntry = null, [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string? FabricApiVersion = null, [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string? OptiFabricVersion = null)
{
	[JsonIgnore]
	public string? OptiFineVersion => OptiFineEntry?["InstanceName"]?.Value<string>()?.AfterLast("-OptiFine_");

	public McComponents(string minecraftVersion, JObject? optiFineEntry, string? forgeVersion, string? neoForgeVersion, string? fabricVersion, string? fabricApiVersion, string? optiFabricVersion, string? liteLoaderVersion)
		: this(minecraftVersion, new Dictionary<ModLoaders, string>(), optiFineEntry, fabricApiVersion, optiFabricVersion)
	{
		if (forgeVersion != null)
		{
			ModLoaderVersions[ModLoaders.Forge] = forgeVersion;
		}
		if (neoForgeVersion != null)
		{
			ModLoaderVersions[ModLoaders.NeoForge] = neoForgeVersion;
		}
		if (fabricVersion != null)
		{
			ModLoaderVersions[ModLoaders.Fabric] = fabricVersion;
		}
		if (liteLoaderVersion != null)
		{
			ModLoaderVersions[ModLoaders.LiteLoader] = liteLoaderVersion;
		}
	}

	public bool IsEquivalentTo(McComponents other)
	{
		McComponents other2 = other;
		string value;
		if (MinecraftVersion == other2.MinecraftVersion && OptiFineVersion == other2.OptiFineVersion && FabricApiVersion == other2.FabricApiVersion && OptiFabricVersion == other2.OptiFabricVersion && ModLoaderVersions.Count == other2.ModLoaderVersions.Count)
		{
			return ModLoaderVersions.All<KeyValuePair<ModLoaders, string>>((KeyValuePair<ModLoaders, string> entry) => other2.ModLoaderVersions.TryGetValue(entry.Key, out value) && value == entry.Value);
		}
		return false;
	}
}
