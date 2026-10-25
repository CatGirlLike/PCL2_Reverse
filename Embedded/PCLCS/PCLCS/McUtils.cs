using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MeloongCore.Extensions;

namespace PCLCS;

public static class McUtils
{
	public static int CompareVersion(string left, string right)
	{
		if (left == right)
		{
			return 0;
		}
		if (left == "未知版本" || right == "未知版本")
		{
			if (!(left == "未知版本"))
			{
				return -1;
			}
			return 1;
		}
		left = left.Lower();
		right = right.Lower();
		List<string> list = left.Replace("快照", "snapshot").Replace("预览版", "pre").RegexSearch("[a-z]+|[0-9]+")
			.ToList();
		List<string> list2 = right.Replace("快照", "snapshot").Replace("预览版", "pre").RegexSearch("[a-z]+|[0-9]+")
			.ToList();
		int num = 0;
		int num2;
		int num3;
		while (true)
		{
			if (num >= list.Count && num >= list2.Count)
			{
				return string.Compare(left, right, StringComparison.Ordinal);
			}
			string text = ((num < list.Count) ? list[num] : "0");
			string text2 = ((num < list2.Count) ? list2[num] : "0");
			if (!(text == text2))
			{
				num2 = text switch
				{
					"rc" => -1, 
					"pre" => -2, 
					"snapshot" => -3, 
					"experimental" => -4, 
					_ => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0, 
				};
				num3 = text2 switch
				{
					"rc" => -1, 
					"pre" => -2, 
					"snapshot" => -3, 
					"experimental" => -4, 
					_ => int.TryParse(text2, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result2) ? result2 : 0, 
				};
				if (num2 == 0 && num3 == 0)
				{
					return string.Compare(text, text2, StringComparison.Ordinal);
				}
				if (num2 != num3)
				{
					break;
				}
			}
			num++;
		}
		return num2.CompareTo(num3);
	}

	public static bool CompareVersionGE(string left, string right)
	{
		return CompareVersion(left, right) >= 0;
	}
}
