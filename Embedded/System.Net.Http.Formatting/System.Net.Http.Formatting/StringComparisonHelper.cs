using System.Web.Http;

namespace System.Net.Http.Formatting;

internal static class StringComparisonHelper
{
	public static bool IsDefined(StringComparison value)
	{
		if (value != 0 && value != StringComparison.CurrentCultureIgnoreCase && value != StringComparison.InvariantCulture && value != StringComparison.InvariantCultureIgnoreCase && value != StringComparison.Ordinal)
		{
			return value == StringComparison.OrdinalIgnoreCase;
		}
		return true;
	}

	public static void Validate(StringComparison value, string parameterName)
	{
		if (!IsDefined(value))
		{
			throw Error.InvalidEnumArgument(parameterName, (int)value, typeof(StringComparison));
		}
	}
}
