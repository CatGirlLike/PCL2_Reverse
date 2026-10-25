using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MeloongCore;
using MeloongCore.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PCLCS;

public sealed class McVersion
{
	private string _vanillaName = "Unknown";

	public Version Vanilla = new Version(9999, 0, 0);

	public bool Reliable;

	public string? OptiFine;

	public bool OptiFineAsMod;

	public DateTime ReleaseTime = new DateTime(1970, 1, 1, 15, 0, 0);

	public bool IsFool;

	public bool IsSnapshot;

	public string VanillaName
	{
		get
		{
			return _vanillaName;
		}
		set
		{
			_vanillaName = value ?? "Unknown";
			Vanilla = NameToVersion(_vanillaName);
		}
	}

	public bool Valid => Vanilla.Major < 1000;

	public int Drop
	{
		get
		{
			if (!Valid)
			{
				return 209;
			}
			return Vanilla.Major * 10 + Vanilla.Minor;
		}
	}

	public ModLoaders ModLoaders => ModLoaderVersions.Keys.Aggregate(ModLoaders.None, (ModLoaders result, ModLoaders loader) => result | loader);

	public Dictionary<ModLoaders, string> ModLoaderVersions { get; } = new Dictionary<ModLoaders, string>();


	public bool HasOptiFine => OptiFine != null;

	public int OptiFineCode
	{
		get
		{
			string optiFine = OptiFine;
			bool flag = ((optiFine == null || optiFine == "未知版本") ? true : false);
			if (flag || OptiFine.Length == 0)
			{
				return 0;
			}
			int num = char.ToUpperInvariant(OptiFine[0]) - 65 + 1;
			num = num * 100 + (int.TryParse(OptiFine.Substring(1).RegexSeek("\\d+"), out var result) ? result : 0);
			num *= 100;
			string text = OptiFine.RegexSeek("(?i)(pre|beta)\\d*");
			if (text == null)
			{
				return num + 99;
			}
			int result2;
			int num2 = ((!int.TryParse(text.RegexSeek("\\d+"), out result2)) ? 1 : result2);
			return num + (text.StartsWithF("pre", ignoreCase: true) ? 50 : 0) + num2;
		}
	}

	public int ForgelikeCode
	{
		get
		{
			if (!ModLoaderVersions.TryGetValue(ModLoaders.Forge, out string value))
			{
				ModLoaderVersions.TryGetValue(ModLoaders.NeoForge, out value);
			}
			if ((value == null || value == "未知版本") ? true : false)
			{
				return 0;
			}
			int result;
			int[] array = (from segment in value.RegexSearch("\\d+")
				select int.TryParse(segment, out result) ? result : 0).ToArray();
			int num = array.Length;
			if (num < 4)
			{
				return num switch
				{
					3 => array[0] * 1000000 + array[1] * 10000 + array[2], 
					2 => array[0] * 1000000 + array[1] * 10000, 
					1 => array[0] * 1000000, 
					_ => 0, 
				};
			}
			return array[0] * 1000000 + array[1] * 10000 + array[3];
		}
	}

	public McVersion(string vanillaName, bool reliable = true)
	{
		Reliable = reliable;
		VanillaName = vanillaName;
		IsFool = IsFoolName(VanillaName);
		IsSnapshot = IsSnapshotName(VanillaName);
	}

	public McVersion(McComponents components)
		: this(components.MinecraftVersion)
	{
		OptiFine = components.OptiFineVersion?.AfterLast("HD_").AfterFirst("_");
		foreach (KeyValuePair<ModLoaders, string> modLoaderVersion in components.ModLoaderVersions)
		{
			ModLoaderVersions[modLoaderVersion.Key] = modLoaderVersion.Value;
		}
	}

	public McVersion(string instanceName, JObject json, JObject? jarVersionJson = null)
	{
		string text = json.ToString(Formatting.None);
		DateTime releaseTime = new DateTime(1970, 1, 1, 15, 0, 0);
		try
		{
			JToken jToken = json["releaseTime"];
			if (jToken != null)
			{
				releaseTime = jToken.ToObject<DateTime>();
			}
		}
		catch
		{
		}
		bool reliable = true;
		string text2 = (GetVanillaName(instanceName, json, jarVersionJson, releaseTime, ref reliable) ?? "Unknown").Replace("_unobfuscated", "").Replace(" Unobfuscated", "");
		Reliable = reliable;
		VanillaName = text2;
		ReleaseTime = releaseTime;
		string text3 = json.Value<string>("type") ?? "";
		OptiFine = (text.ContainsIgnoreCase("optifine") ? (text.RegexSeek("(?<=HD_U_)[^\\\":/]+") ?? "未知版本") : null);
		DateTime dateTime = releaseTime.ToUniversalTime().AddHours(2.0);
		IsFool = text3 == "fool" || IsFoolName(text2) || (dateTime.Month == 4 && dateTime.Day == 1 && text3 == "snapshot");
		bool isEnabled = IsSnapshotName(text2) || instanceName.ContainsIgnoreCase("combat");
		if (!isEnabled)
		{
			bool flag;
			switch (text3)
			{
			case "snapshot":
			case "pending":
			case "fool":
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			isEnabled = flag;
		}
		IsSnapshot = isEnabled;
		if (!Reliable)
		{
			Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel> handler = new Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel>(17, 1, out isEnabled);
			if (isEnabled)
			{
				handler.AppendLiteral("无法完全确认 MC 版本号的版本：");
				handler.AppendFormatted(VanillaName);
			}
			Logger.Info(ref handler, LogBehavior.None, "E:\\Projects\\PCL\\本体代码\\PCLCS\\Minecraft\\McVersion.cs");
		}
		if (text.ContainsIgnoreCase("liteloader"))
		{
			ModLoaderVersions[ModLoaders.LiteLoader] = text.RegexSeek("(?<=com\\.mumfrey:liteloader:)[^\\\":/]+") ?? "未知版本";
		}
		if (text.ContainsIgnoreCase("net.fabricmc:fabric-loader") || text.ContainsIgnoreCase("org.quiltmc:quilt-loader"))
		{
			ModLoaderVersions[ModLoaders.Fabric] = text.RegexSeek("(?<=(net\\.fabricmc:fabric-loader:|org\\.quiltmc:quilt-loader:))[0-9\\.]+(\\+build.[0-9]+)?")?.Replace("+build", "") ?? "未知版本";
		}
		else if (text.ContainsIgnoreCase("net.neoforge"))
		{
			ModLoaderVersions[ModLoaders.NeoForge] = FindArgumentValue(json, "--fml.neoForgeVersion") ?? FindArgumentValue(json, "--fml.forgeVersion") ?? "未知版本";
		}
		else if (text.ContainsIgnoreCase("minecraftforge"))
		{
			ModLoaderVersions[ModLoaders.Forge] = text.RegexSeek("(?<=forge:[0-9\\.]+(?:_pre[0-9]*)?-)[0-9\\.]+") ?? text.RegexSeek("(?<=net\\.minecraftforge:minecraftforge:)[0-9\\.]+") ?? text.RegexSeek("(?<=net\\.minecraftforge:fmlloader:[0-9\\.]+-)[0-9\\.]+") ?? "未知版本";
		}
	}

	private static string? GetVanillaName(string instanceName, JObject json, JObject? jarVersionJson, DateTime releaseTime, ref bool reliable)
	{
		int year = releaseTime.Year;
		if (year > 2000 && year < 2013)
		{
			return "Old";
		}
		if (json.Value<string>("type") == "pending")
		{
			return "pending";
		}
		JToken jToken = json["clientVersion"];
		if (jToken != null)
		{
			return jToken.ToString();
		}
		if (json["patches"] is JArray source)
		{
			JToken jToken2 = source.OfType<JObject>().FirstOrDefault((JObject patch) => patch.Value<string>("id") == "game")?["version"];
			if (jToken2 != null)
			{
				return jToken2.ToString();
			}
		}
		string text = FindArgumentValue(json, "--fml.mcVersion");
		if (text != null && text.Length > 0)
		{
			return text;
		}
		string text2 = json["inheritsFrom"]?.ToString() ?? "";
		if (text2.Length > 0)
		{
			string text3 = json["jar"]?.ToString() ?? "";
			if (text3.Length <= 0)
			{
				return text2;
			}
			return text3;
		}
		string text4 = json["downloads"]?.ToString(Formatting.None).RegexSeek("(?<=launcher.mojang.com/mc/game/)[^/]*");
		if (text4 != null)
		{
			return text4;
		}
		string str = json["libraries"].ToString();
		string[] array = new string[4] { "(?<=net.minecraftforge:forge:)[0-9]{1,2}.[0-9+.]+", "(?<=net.minecraftforge:fmlloader:)[0-9]{1,2}.[0-9+.]+", "(?<=optifine:OptiFine:)[0-9]{1,2}.[0-9+.]+", "(?<=((fabricmc)|(quiltmc)):intermediary:)[^\\\"]*" };
		foreach (string pattern in array)
		{
			string text5 = str.RegexSeek(pattern);
			if (text5 != null)
			{
				return text5;
			}
		}
		JToken jToken3 = json["jar"];
		if (jToken3 != null)
		{
			return jToken3.ToString();
		}
		JToken jToken4 = jarVersionJson?["name"];
		bool isEnabled;
		if (jToken4 != null)
		{
			string text6 = jToken4.ToString();
			if (text6 != null && text6.Length < 32)
			{
				Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel> handler = new Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel>(31, 1, out isEnabled);
				if (isEnabled)
				{
					handler.AppendLiteral("从版本 jar 中的 version.json 获取到版本号：");
					handler.AppendFormatted(text6);
				}
				Logger.Info(ref handler, LogBehavior.None, "E:\\Projects\\PCL\\本体代码\\PCLCS\\Minecraft\\McVersion.cs");
				return text6;
			}
		}
		string pattern2 = "(([1-9][0-9]w[0-9]{2}[a-g])|((1|[2-9][0-9])\\.[0-9]+(\\.[0-9]+)?(-(pre|rc|snapshot-?)[1-9]*| Pre-Release( [1-9])?)?))(_unobfuscated)?";
		string text7 = json["id"].ToString().RegexSeek(pattern2, RegexOptions.IgnoreCase);
		if (text7 != null)
		{
			return text7;
		}
		Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel> handler2 = new Logger.LogInterpolatedStringHandler<Logger.InfoLogLevel>(17, 1, out isEnabled);
		if (isEnabled)
		{
			handler2.AppendLiteral("无法完全确认 MC 版本号的版本：");
			handler2.AppendFormatted(instanceName);
		}
		Logger.Info(ref handler2, LogBehavior.None, "E:\\Projects\\PCL\\本体代码\\PCLCS\\Minecraft\\McVersion.cs");
		reliable = false;
		string text8 = instanceName.RegexSeek(pattern2, RegexOptions.IgnoreCase);
		if (text8 != null)
		{
			return text8;
		}
		JObject obj = (JObject)json.DeepClone();
		obj.Remove("libraries");
		return obj.ToString(Formatting.None).RegexSeek(pattern2, RegexOptions.IgnoreCase);
	}

	private static bool IsFoolName(string name)
	{
		name = name.Lower();
		bool flag = name.StartsWithF("2.0");
		bool flag2;
		if (!flag)
		{
			if (name != null)
			{
				int length = name.Length;
				if (length <= 11)
				{
					if (length != 6)
					{
						if (length != 9)
						{
							if (length == 11)
							{
								char c = name[1];
								if (c != '3')
								{
									if (c == '4' && name == "24w14potato")
									{
										goto IL_0115;
									}
								}
								else if (name == "23w13a_or_b")
								{
									goto IL_0115;
								}
							}
						}
						else if (name == "1.rv-pre1")
						{
							goto IL_0115;
						}
					}
					else
					{
						char c = name[1];
						if (c != '0')
						{
							if (c != '5')
							{
								if (c == '6' && name == "26w14a")
								{
									goto IL_0115;
								}
							}
							else if (name == "15w14a")
							{
								goto IL_0115;
							}
						}
						else if (name == "20w14∞")
						{
							goto IL_0115;
						}
					}
				}
				else if (length != 14)
				{
					if (length != 18)
					{
						if (length == 20 && name == "22w13oneblockatatime")
						{
							goto IL_0115;
						}
					}
					else if (name == "3d shareware v1.34")
					{
						goto IL_0115;
					}
				}
				else if (name == "25w14craftmine")
				{
					goto IL_0115;
				}
			}
			flag2 = false;
			goto IL_011b;
		}
		goto IL_011d;
		IL_0115:
		flag2 = true;
		goto IL_011b;
		IL_011b:
		flag = flag2;
		goto IL_011d;
		IL_011d:
		if (!flag)
		{
			return name.StartsWithF("20w14inf");
		}
		return true;
	}

	private static bool IsSnapshotName(string name)
	{
		return new string[6] { "w", "snapshot", "rc", "pre", "experimental", "-" }.Any(name.ContainsIgnoreCase);
	}

	private static string? FindArgumentValue(JObject json, string name)
	{
		if (!(json["arguments"]?["game"] is JArray jArray))
		{
			return null;
		}
		for (int i = 0; i + 1 < jArray.Count; i++)
		{
			if (jArray[i]?.ToString() == name)
			{
				return jArray[i + 1]?.ToString();
			}
		}
		return null;
	}

	public static bool IsStandardFormat(string? version)
	{
		if (version == null)
		{
			return false;
		}
		if (Regex.IsMatch(version, "^1\\.\\d"))
		{
			return true;
		}
		if (double.TryParse(version.RegexSeek("^[2-9]\\d\\.\\d+"), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return result >= 26.0;
		}
		return false;
	}

	public static int VersionToDrop(string? vanillaName)
	{
		if (vanillaName == null)
		{
			return 209;
		}
		Version version = NameToVersion(vanillaName);
		if (version.Major < 1000)
		{
			return version.Major * 10 + version.Minor;
		}
		return 209;
	}

	public static string DropToVersion(int drop)
	{
		if (drop >= 250)
		{
			return $"{drop / 10}.{drop % 10}";
		}
		if (drop != 209)
		{
			return $"1.{drop / 10}";
		}
		Logger.Warn(new Exception("尝试将旧快照版 Drop 序数转换为版本字符串，这不应该发生！"), null, LogBehavior.ToastIfDebug, "E:\\Projects\\PCL\\本体代码\\PCLCS\\Minecraft\\McVersion.cs");
		return "1.20";
	}

	public static Version NameToVersion(string? name)
	{
		if (name == null)
		{
			return new Version(9999, 0, 0);
		}
		name = name.Lower().Replace("_unobfuscated", "").Replace(" unobfuscated", "");
		if (name.StartsWithF("2.0"))
		{
			return NameToVersion("1.5.1");
		}
		if (name == "15w14a")
		{
			return NameToVersion("1.8.3");
		}
		if (name.ContainsIgnoreCase(".rv-pre"))
		{
			return NameToVersion("1.9.2");
		}
		if (name.ContainsIgnoreCase("shareware"))
		{
			return NameToVersion("1.13.2");
		}
		if (name.StartsWithF("20w14") && name != "20w14a")
		{
			return NameToVersion("1.15.2");
		}
		if (name.ContainsIgnoreCase("oneblockatatime"))
		{
			return NameToVersion("1.18.2");
		}
		if (name.ContainsIgnoreCase("23w13a") && name != "20w13a")
		{
			return NameToVersion("1.19.4");
		}
		switch (name)
		{
		case "24w14potato":
			return NameToVersion("1.20.4");
		case "25w14craftmine":
			return NameToVersion("1.21.4");
		case "26w14a":
			return NameToVersion("26.1.1");
		default:
		{
			string[] segments2 = name.Split(new char[4] { ' ', '_', '-', '.' }, StringSplitOptions.None);
			if (name.StartsWithF("1."))
			{
				return new Version(ParseSegment(segments2, 1), 0, ParseSegment(segments2, 2));
			}
			if (name.RegexCheck("^[2-9][0-9]\\."))
			{
				return new Version(ParseSegment(segments2, 0), ParseSegment(segments2, 1), ParseSegment(segments2, 2));
			}
			return new Version(9999, 0, 0);
		}
		}
		static int ParseSegment(string[] segments, int index)
		{
			if (index >= segments.Length)
			{
				return 0;
			}
			if (!int.TryParse(segments[index].RegexSeek("^\\d+"), out var result))
			{
				return 0;
			}
			return result;
		}
	}
}
