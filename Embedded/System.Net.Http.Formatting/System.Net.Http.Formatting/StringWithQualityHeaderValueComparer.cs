using System.Collections.Generic;
using System.Net.Http.Headers;

namespace System.Net.Http.Formatting;

internal class StringWithQualityHeaderValueComparer : IComparer<StringWithQualityHeaderValue>
{
	private static readonly StringWithQualityHeaderValueComparer _qualityComparer = new StringWithQualityHeaderValueComparer();

	public static StringWithQualityHeaderValueComparer QualityComparer => _qualityComparer;

	private StringWithQualityHeaderValueComparer()
	{
	}

	public int Compare(StringWithQualityHeaderValue stringWithQuality1, StringWithQualityHeaderValue stringWithQuality2)
	{
		double num = stringWithQuality1.Quality ?? 1.0;
		double num2 = stringWithQuality2.Quality ?? 1.0;
		double num3 = num - num2;
		if (num3 < 0.0)
		{
			return -1;
		}
		if (num3 > 0.0)
		{
			return 1;
		}
		if (!string.Equals(stringWithQuality1.Value, stringWithQuality2.Value, StringComparison.OrdinalIgnoreCase))
		{
			if (string.Equals(stringWithQuality1.Value, "*", StringComparison.OrdinalIgnoreCase))
			{
				return -1;
			}
			if (string.Equals(stringWithQuality2.Value, "*", StringComparison.OrdinalIgnoreCase))
			{
				return 1;
			}
		}
		return 0;
	}
}
