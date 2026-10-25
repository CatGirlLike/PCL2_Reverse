using System.Linq;

namespace System.Net.Http;

internal static class TypeExtensions
{
	public static Type ExtractGenericInterface(this Type queryType, Type interfaceType)
	{
		Func<Type, bool> func = (Type t) => t.IsGenericType() && t.GetGenericTypeDefinition() == interfaceType;
		if (!func(queryType))
		{
			return queryType.GetInterfaces().FirstOrDefault(func);
		}
		return queryType;
	}

	public static bool IsGenericType(this Type type)
	{
		return type.IsGenericType;
	}

	public static bool IsInterface(this Type type)
	{
		return type.IsInterface;
	}

	public static bool IsValueType(this Type type)
	{
		return type.IsValueType;
	}
}
