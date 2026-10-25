using System.Collections.Generic;
using MeloongCore;

namespace PCLCS;

public static class Configs
{
	public static readonly DynamicConfigEntry<McInstallManifest> McInstallManifest;

	public static readonly ConfigEntry<int> JavaConfigVersion;

	public static readonly ConfigEntry<ConcurrentList<Java>> JavaList;

	public static readonly ConfigEntry<List<string>> JavaRemovedList;

	public static readonly DynamicConfigEntry<bool> JavaMigrated;

	public static readonly DynamicConfigEntry<Java> JavaForced;

	public static readonly ConfigEntry<bool> HintModifyInstanceWarning;

	static Configs()
	{
		McInstallManifest = new DynamicConfigEntry<McInstallManifest>("MinecraftInstallManifest", null);
		JavaConfigVersion = new ConfigEntry<int>("JavaConfigVersion", 0);
		JavaList = new ConfigEntry<ConcurrentList<Java>>("JavaList", new ConcurrentList<Java>());
		JavaRemovedList = new ConfigEntry<List<string>>("JavaRemovedList", new List<string>());
		JavaMigrated = new DynamicConfigEntry<bool>("InstanceMigratedJava", defaultValue: false);
		JavaForced = new DynamicConfigEntry<Java>("InstanceForcedJava", null);
		HintModifyInstanceWarning = new ConfigEntry<bool>("HintModifyInstanceWarning", defaultValue: false);
		JavaForced.PreviewGet += delegate(PreviewGetEventArgs<Java> args, IConfigProvider provider)
		{
			if (args.Value != null && !JavaList.Get().Contains(args.Value))
			{
				args.Value = null;
				JavaForced.Reset(provider);
			}
		};
	}
}
