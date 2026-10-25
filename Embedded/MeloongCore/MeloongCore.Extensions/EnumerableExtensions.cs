using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace MeloongCore.Extensions;

public static class EnumerableExtensions
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass1_0<T> where T : notnull
	{
		public Func<T, T, bool> comparer;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass1_1<T> where T : notnull
	{
		public T item;

		public _003C_003Ec__DisplayClass1_0<T> CS_0024_003C_003E8__locals1;

		internal bool _003CDistinct_003Eb__0(T s)
		{
			return CS_0024_003C_003E8__locals1.comparer(s, item);
		}
	}

	[CompilerGenerated]
	private sealed class _003CDistinct_003Ed__1<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable where T : notnull
	{
		private int _003C_003E1__state;

		private T _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Func<T, T, bool> comparer;

		public Func<T, T, bool> _003C_003E3__comparer;

		private IList<T> list;

		public IList<T> _003C_003E3__list;

		private _003C_003Ec__DisplayClass1_0<T> _003C_003E8__1;

		private List<T> _003Cseen_003E5__2;

		private IEnumerator<T> _003C_003E7__wrap2;

		T IEnumerator<T>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CDistinct_003Ed__1(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E8__1 = null;
			_003Cseen_003E5__2 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E8__1 = new _003C_003Ec__DisplayClass1_0<T>();
					_003C_003E8__1.comparer = comparer;
					_003Cseen_003E5__2 = new List<T>();
					_003C_003E7__wrap2 = list.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap2.MoveNext())
				{
					_003C_003Ec__DisplayClass1_1<T> CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass1_1<T>
					{
						CS_0024_003C_003E8__locals1 = _003C_003E8__1,
						item = _003C_003E7__wrap2.Current
					};
					if (!_003Cseen_003E5__2.Any((T s) => CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.comparer(s, CS_0024_003C_003E8__locals0.item)))
					{
						_003Cseen_003E5__2.Add(CS_0024_003C_003E8__locals0.item);
						_003C_003E2__current = CS_0024_003C_003E8__locals0.item;
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap2 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			_003CDistinct_003Ed__1<T> _003CDistinct_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CDistinct_003Ed__ = this;
			}
			else
			{
				_003CDistinct_003Ed__ = new _003CDistinct_003Ed__1<T>(0);
			}
			_003CDistinct_003Ed__.list = _003C_003E3__list;
			_003CDistinct_003Ed__.comparer = _003C_003E3__comparer;
			return _003CDistinct_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CDistinctBy_003Ed__2<TSource, TKey> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable where TSource : notnull where TKey : notnull
	{
		private int _003C_003E1__state;

		private TSource _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerable<TSource> source;

		public IEnumerable<TSource> _003C_003E3__source;

		private Func<TSource, TKey> selector;

		public Func<TSource, TKey> _003C_003E3__selector;

		private HashSet<TKey> _003Cseen_003E5__2;

		private IEnumerator<TSource> _003C_003E7__wrap2;

		TSource IEnumerator<TSource>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CDistinctBy_003Ed__2(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003Cseen_003E5__2 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003Cseen_003E5__2 = new HashSet<TKey>();
					_003C_003E7__wrap2 = source.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap2.MoveNext())
				{
					TSource current = _003C_003E7__wrap2.Current;
					if (_003Cseen_003E5__2.Add(selector(current)))
					{
						_003C_003E2__current = current;
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap2 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
		{
			_003CDistinctBy_003Ed__2<TSource, TKey> _003CDistinctBy_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CDistinctBy_003Ed__ = this;
			}
			else
			{
				_003CDistinctBy_003Ed__ = new _003CDistinctBy_003Ed__2<TSource, TKey>(0);
			}
			_003CDistinctBy_003Ed__.source = _003C_003E3__source;
			_003CDistinctBy_003Ed__.selector = _003C_003E3__selector;
			return _003CDistinctBy_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<TSource>)this).GetEnumerator();
		}
	}

	public static TSource? FirstOrDefault<TSource>(this IEnumerable<TSource> source, params Func<TSource, bool>[] predicates)
	{
		foreach (Func<TSource, bool> func in predicates)
		{
			foreach (TSource item in source)
			{
				if (func(item))
				{
					return item;
				}
			}
		}
		return default(TSource);
	}

	[IteratorStateMachine(typeof(_003CDistinct_003Ed__1<>))]
	public static IEnumerable<T> Distinct<T>(this IList<T> list, Func<T, T, bool> comparer)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDistinct_003Ed__1<T>(-2)
		{
			_003C_003E3__list = list,
			_003C_003E3__comparer = comparer
		};
	}

	[IteratorStateMachine(typeof(_003CDistinctBy_003Ed__2<, >))]
	public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDistinctBy_003Ed__2<TSource, TKey>(-2)
		{
			_003C_003E3__source = source,
			_003C_003E3__selector = selector
		};
	}

	public static ParallelQuery<TSource> DistinctBy<TSource, TKey>(this ParallelQuery<TSource> source, Func<TSource, TKey> selector)
	{
		Func<TSource, TKey> selector2 = selector;
		ConcurrentDictionary<TKey, bool> seen = new ConcurrentDictionary<TKey, bool>();
		return source.Where((TSource element) => seen.TryAdd(selector2(element), value: true));
	}

	public static List<TSource> MaxByAll<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector) where TKey : IComparable<TKey>
	{
		List<TSource> list = new List<TSource>();
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return list;
		}
		TSource current = enumerator.Current;
		TKey other = selector(current);
		list.Add(current);
		while (enumerator.MoveNext())
		{
			TSource current2 = enumerator.Current;
			TKey val = selector(current2);
			int num = val.CompareTo(other);
			if (num > 0)
			{
				other = val;
				list.Clear();
				list.Add(current2);
			}
			else if (num == 0)
			{
				list.Add(current2);
			}
		}
		return list;
	}

	public static List<TSource> MaxByAll<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
	{
		List<TSource> list = new List<TSource>();
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return list;
		}
		TSource current = enumerator.Current;
		TKey y = selector(current);
		list.Add(current);
		while (enumerator.MoveNext())
		{
			TSource current2 = enumerator.Current;
			TKey val = selector(current2);
			int num = comparer.Compare(val, y);
			if (num > 0)
			{
				y = val;
				list.Clear();
				list.Add(current2);
			}
			else if (num == 0)
			{
				list.Add(current2);
			}
		}
		return list;
	}

	public static List<TSource> MinByAll<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector) where TKey : IComparable<TKey>
	{
		List<TSource> list = new List<TSource>();
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return list;
		}
		TSource current = enumerator.Current;
		TKey other = selector(current);
		list.Add(current);
		while (enumerator.MoveNext())
		{
			TSource current2 = enumerator.Current;
			TKey val = selector(current2);
			int num = val.CompareTo(other);
			if (num < 0)
			{
				other = val;
				list.Clear();
				list.Add(current2);
			}
			else if (num == 0)
			{
				list.Add(current2);
			}
		}
		return list;
	}

	public static List<TSource> MinByAll<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
	{
		List<TSource> list = new List<TSource>();
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return list;
		}
		TSource current = enumerator.Current;
		TKey y = selector(current);
		list.Add(current);
		while (enumerator.MoveNext())
		{
			TSource current2 = enumerator.Current;
			TKey val = selector(current2);
			int num = comparer.Compare(val, y);
			if (num < 0)
			{
				y = val;
				list.Clear();
				list.Add(current2);
			}
			else if (num == 0)
			{
				list.Add(current2);
			}
		}
		return list;
	}

	public static TSource? MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector) where TKey : IComparable<TKey>
	{
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return default(TSource);
		}
		TSource current = enumerator.Current;
		TKey other = selector(current);
		while (enumerator.MoveNext())
		{
			TKey val = selector(enumerator.Current);
			if (val.CompareTo(other) > 0)
			{
				current = enumerator.Current;
				other = val;
			}
		}
		return current;
	}

	public static TSource? MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
	{
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return default(TSource);
		}
		TSource current = enumerator.Current;
		TKey y = selector(current);
		while (enumerator.MoveNext())
		{
			TKey val = selector(enumerator.Current);
			if (comparer.Compare(val, y) > 0)
			{
				current = enumerator.Current;
				y = val;
			}
		}
		return current;
	}

	public static TSource? MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector) where TKey : IComparable<TKey>
	{
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return default(TSource);
		}
		TSource current = enumerator.Current;
		TKey other = selector(current);
		while (enumerator.MoveNext())
		{
			TKey val = selector(enumerator.Current);
			if (val.CompareTo(other) < 0)
			{
				current = enumerator.Current;
				other = val;
			}
		}
		return current;
	}

	public static TSource? MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
	{
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return default(TSource);
		}
		TSource current = enumerator.Current;
		TKey y = selector(current);
		while (enumerator.MoveNext())
		{
			TKey val = selector(enumerator.Current);
			if (comparer.Compare(val, y) < 0)
			{
				current = enumerator.Current;
				y = val;
			}
		}
		return current;
	}

	public static IEnumerable<T> Ordered<T>(this IEnumerable<T> source) where T : IComparable<T>
	{
		return source.OrderBy((T x) => x);
	}

	public static IEnumerable<T> OrderedDescending<T>(this IEnumerable<T> source) where T : IComparable<T>
	{
		return source.OrderByDescending((T x) => x);
	}

	public static string Join(this IEnumerable list, char split)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (object item in list)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				stringBuilder.Append(split);
			}
			if (item != null)
			{
				stringBuilder.Append(item.ToString());
			}
		}
		return stringBuilder.ToString();
	}

	public static string Join(this IEnumerable list, string split)
	{
		if (split.IsSingle())
		{
			return list.Join(split[0]);
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (object item in list)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				stringBuilder.Append(split);
			}
			if (item != null)
			{
				stringBuilder.Append(item);
			}
		}
		return stringBuilder.ToString();
	}

	public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
	{
		TValue value;
		return dict.TryRemove(key, out value);
	}

	public static Dictionary<T, T> ToDictionary<T>(this T[,] arr) where T : notnull
	{
		Dictionary<T, T> dictionary = new Dictionary<T, T>();
		if (arr.Length == 0)
		{
			return dictionary;
		}
		if (arr.GetLength(1) != 2)
		{
			throw new ArgumentException("数组必须为两列，第一列为 Key，第二列为 Value。");
		}
		for (int i = 0; i < arr.GetLength(0); i++)
		{
			dictionary[arr[i, 0]] = arr[i, 1];
		}
		return dictionary;
	}

	public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue? defaultValue = default(TValue?))
	{
		if (!dict.TryGetValue(key, out TValue value))
		{
			return defaultValue;
		}
		return value;
	}

	public static void AddIntoValueCollection<TKey, TValue>(this IDictionary<TKey, List<TValue>> dict, TKey key, TValue value)
	{
		if (dict.TryGetValue(key, out List<TValue> value2))
		{
			value2.Add(value);
			return;
		}
		dict.Add(key, new List<TValue>(1) { value });
	}

	public static IEnumerable<T> Flags<T>(this T value) where T : struct, Enum
	{
		return from element in EnumUtils.GetAllFlags<T>()
			where value.HasFlag(element)
			select element;
	}

	public static void RemoveIf<T>(this ICollection<T> source, Func<T, bool> predicate)
	{
		foreach (T item in source.Where(predicate).ToList())
		{
			source.Remove(item);
		}
	}

	public static void RemoveAll<T>(this ICollection<T> source, IEnumerable<T> itemsToRemove)
	{
		foreach (T item in itemsToRemove)
		{
			source.Remove(item);
		}
	}

	public static void KeepIf<T>(this ICollection<T> source, Func<T, bool> predicate)
	{
		Func<T, bool> predicate2 = predicate;
		foreach (T item in source.Where((T item) => !predicate2(item)).ToList())
		{
			source.Remove(item);
		}
	}

	public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T element)
	{
		return source.Except(new _003C_003Ez__ReadOnlySingleElementList<T>(element));
	}

	public static bool IsSingle<T>(this IEnumerable<T>? source)
	{
		if (source == null)
		{
			return false;
		}
		if (source is IList<T> list)
		{
			return list.Count == 1;
		}
		using IEnumerator<T> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return false;
		}
		if (enumerator.MoveNext())
		{
			return false;
		}
		return true;
	}

	public static IEnumerable<T> ForAll<T>(this IEnumerable<T> source, Action<T> action)
	{
		foreach (T item in source)
		{
			action(item);
		}
		return source;
	}
}
