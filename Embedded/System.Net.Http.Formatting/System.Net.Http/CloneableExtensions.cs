namespace System.Net.Http;

internal static class CloneableExtensions
{
	internal static T Clone<T>(this T value) where T : ICloneable
	{
		return (T)value.Clone();
	}
}
