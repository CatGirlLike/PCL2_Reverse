using System.Collections.ObjectModel;
using System.Linq;

namespace System.Collections.Generic;

internal static class CollectionExtensions
{
	public static T[] AppendAndReallocate<T>(this T[] array, T value)
	{
		int num = array.Length;
		T[] array2 = new T[num + 1];
		array.CopyTo(array2, 0);
		array2[num] = value;
		return array2;
	}

	public static T[] AsArray<T>(this IEnumerable<T> values)
	{
		T[] array = values as T[];
		if (array == null)
		{
			array = values.ToArray();
		}
		return array;
	}

	public static Collection<T> AsCollection<T>(this IEnumerable<T> enumerable)
	{
		if (enumerable is Collection<T> result)
		{
			return result;
		}
		IList<T> list = enumerable as IList<T>;
		if (list == null)
		{
			list = new List<T>(enumerable);
		}
		return new Collection<T>(list);
	}

	public static IList<T> AsIList<T>(this IEnumerable<T> enumerable)
	{
		if (enumerable is IList<T> result)
		{
			return result;
		}
		return new List<T>(enumerable);
	}

	public static List<T> AsList<T>(this IEnumerable<T> enumerable)
	{
		if (enumerable is List<T> result)
		{
			return result;
		}
		if (enumerable is ListWrapperCollection<T> listWrapperCollection)
		{
			return listWrapperCollection.ItemsList;
		}
		return new List<T>(enumerable);
	}

	public static void RemoveFrom<T>(this List<T> list, int start)
	{
		list.RemoveRange(start, list.Count - start);
	}

	public static T SingleDefaultOrError<T, TArg1>(this IList<T> list, Action<TArg1> errorAction, TArg1 errorArg1)
	{
		switch (list.Count)
		{
		case 0:
			return default(T);
		case 1:
			return list[0];
		default:
			errorAction(errorArg1);
			return default(T);
		}
	}

	public static TMatch SingleOfTypeDefaultOrError<TInput, TMatch, TArg1>(this IList<TInput> list, Action<TArg1> errorAction, TArg1 errorArg1) where TMatch : class
	{
		TMatch val = null;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is TMatch val2)
			{
				if (val != null)
				{
					errorAction(errorArg1);
					return null;
				}
				val = val2;
			}
		}
		return val;
	}

	public static T[] ToArrayWithoutNulls<T>(this ICollection<T> collection) where T : class
	{
		T[] array = new T[collection.Count];
		int num = 0;
		foreach (T item in collection)
		{
			if (item != null)
			{
				array[num] = item;
				num++;
			}
		}
		if (num == collection.Count)
		{
			return array;
		}
		T[] array2 = new T[num];
		Array.Copy(array, array2, num);
		return array2;
	}

	public static Dictionary<TKey, TValue> ToDictionaryFast<TKey, TValue>(this TValue[] array, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(array.Length, comparer);
		foreach (TValue val in array)
		{
			dictionary.Add(keySelector(val), val);
		}
		return dictionary;
	}

	public static Dictionary<TKey, TValue> ToDictionaryFast<TKey, TValue>(this IList<TValue> list, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		if (list is TValue[] array)
		{
			return array.ToDictionaryFast(keySelector, comparer);
		}
		return ToDictionaryFastNoCheck(list, keySelector, comparer);
	}

	public static Dictionary<TKey, TValue> ToDictionaryFast<TKey, TValue>(this IEnumerable<TValue> enumerable, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		if (enumerable is TValue[] array)
		{
			return array.ToDictionaryFast(keySelector, comparer);
		}
		if (enumerable is IList<TValue> list)
		{
			return ToDictionaryFastNoCheck(list, keySelector, comparer);
		}
		Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(comparer);
		foreach (TValue item in enumerable)
		{
			dictionary.Add(keySelector(item), item);
		}
		return dictionary;
	}

	private static Dictionary<TKey, TValue> ToDictionaryFastNoCheck<TKey, TValue>(IList<TValue> list, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		int count = list.Count;
		Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(count, comparer);
		for (int i = 0; i < count; i++)
		{
			TValue val = list[i];
			dictionary.Add(keySelector(val), val);
		}
		return dictionary;
	}
}
