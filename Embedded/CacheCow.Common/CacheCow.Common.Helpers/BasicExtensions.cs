using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheCow.Common.Helpers;

public static class BasicExtensions
{
	public static bool IsIn<T>(this T item, params T[] list)
	{
		if (list == null || list.Length == 0)
		{
			return false;
		}
		return list.Any((T x) => EqualityComparer<T>.Default.Equals(x, item));
	}

	public static Action Chain(this IEnumerable<Action> actions)
	{
		return delegate
		{
			foreach (Action action in actions)
			{
				action();
			}
		};
	}

	public static Func<Task> Chain(this IEnumerable<Func<Task>> actions)
	{
		return async delegate
		{
			foreach (Func<Task> action in actions)
			{
				await action().ConfigureAwait(continueOnCapturedContext: false);
			}
		};
	}

	public static string ToHex(this byte[] data)
	{
		StringBuilder stringBuilder = new StringBuilder(data.Length * 2);
		foreach (byte b in data)
		{
			stringBuilder.AppendFormat("{0:x2}", b);
		}
		return stringBuilder.ToString();
	}

	public static byte[] FromHex(this string hex)
	{
		int length = hex.Length;
		byte[] array = new byte[length / 2];
		for (int i = 0; i < length; i += 2)
		{
			array[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
		}
		return array;
	}
}
