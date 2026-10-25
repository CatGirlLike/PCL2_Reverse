using System;
using System.Collections.Generic;

namespace MeloongCore;

public sealed class ValueRange<T> : IEquatable<ValueRange<T>?> where T : IComparable<T>
{
	public bool IsLowerInclusive;

	public bool IsUpperInclusive;

	public T? Lower { get; private set; }

	public T? Upper { get; private set; }

	public bool HasLower { get; private set; }

	public bool HasUpper { get; private set; }

	public ValueRange(T? lower, T? upper, bool isLowerInclusive, bool isUpperInclusive, bool hasLower, bool hasUpper)
	{
		if (hasLower && lower == null)
		{
			throw new ArgumentNullException("lower");
		}
		Lower = lower;
		if (hasUpper && upper == null)
		{
			throw new ArgumentNullException("upper");
		}
		Upper = upper;
		HasLower = hasLower;
		HasUpper = hasUpper;
		IsLowerInclusive = isLowerInclusive;
		IsUpperInclusive = isUpperInclusive;
		base._002Ector();
	}

	public void ClearLower()
	{
		Lower = default(T);
		HasLower = false;
	}

	public void ClearUpper()
	{
		Upper = default(T);
		HasUpper = false;
	}

	public void SetLower(T lower, bool isInclusive)
	{
		Lower = lower;
		HasLower = true;
		IsLowerInclusive = isInclusive;
	}

	public void SetUpper(T upper, bool isInclusive)
	{
		Upper = upper;
		HasUpper = true;
		IsUpperInclusive = isInclusive;
	}

	public static ValueRange<T> Closed(T lower, T upper)
	{
		return new ValueRange<T>(lower, upper, isLowerInclusive: true, isUpperInclusive: true, hasLower: true, hasUpper: true);
	}

	public static ValueRange<T> ClosedOpen(T lower, T upper)
	{
		return new ValueRange<T>(lower, upper, isLowerInclusive: true, isUpperInclusive: false, hasLower: true, hasUpper: true);
	}

	public static ValueRange<T> OpenClosed(T lower, T upper)
	{
		return new ValueRange<T>(lower, upper, isLowerInclusive: false, isUpperInclusive: true, hasLower: true, hasUpper: true);
	}

	public static ValueRange<T> Open(T lower, T upper)
	{
		return new ValueRange<T>(lower, upper, isLowerInclusive: false, isUpperInclusive: false, hasLower: true, hasUpper: true);
	}

	public static ValueRange<T> Exactly(T value)
	{
		return new ValueRange<T>(value, value, isLowerInclusive: true, isUpperInclusive: true, hasLower: true, hasUpper: true);
	}

	public static ValueRange<T> AtLeast(T lower)
	{
		return new ValueRange<T>(lower, default(T), isLowerInclusive: true, isUpperInclusive: true, hasLower: true, hasUpper: false);
	}

	public static ValueRange<T> GreaterThan(T lower)
	{
		return new ValueRange<T>(lower, default(T), isLowerInclusive: false, isUpperInclusive: true, hasLower: true, hasUpper: false);
	}

	public static ValueRange<T> AtMost(T upper)
	{
		return new ValueRange<T>(default(T), upper, isLowerInclusive: true, isUpperInclusive: true, hasLower: false, hasUpper: true);
	}

	public static ValueRange<T> LessThan(T upper)
	{
		return new ValueRange<T>(default(T), upper, isLowerInclusive: true, isUpperInclusive: false, hasLower: false, hasUpper: true);
	}

	public static ValueRange<T> All()
	{
		return new ValueRange<T>(default(T), default(T), isLowerInclusive: true, isUpperInclusive: true, hasLower: false, hasUpper: false);
	}

	public static ValueRange<T> FromString(string str, Func<string, T>? parseFunc = null)
	{
		if (string.IsNullOrEmpty(str))
		{
			throw new ArgumentException("不能为空");
		}
		bool flag = str[0] == '[';
		bool flag2 = str[str.Length - 1] == ']';
		if (!flag && str[0] != '(')
		{
			throw new ArgumentException("开头必须是 [ 或 (");
		}
		if (!flag2)
		{
			if (str[str.Length - 1] != ')')
			{
				throw new ArgumentException("结尾必须是 ] 或 )");
			}
		}
		int num = str.IndexOfAny(new char[2] { ',', '，' });
		if (num == -1)
		{
			throw new ArgumentException("需要用逗号分隔两个值");
		}
		string text = str.Substring(1, num - 1).Trim();
		int num2 = num + 1;
		string text2 = str.Substring(num2, str.Length - 1 - num2).Trim();
		T lower = default(T);
		T upper = default(T);
		bool hasLower = false;
		bool hasUpper = false;
		if (((text == null || text.Length != 0) && !(text == "-∞")) || 1 == 0)
		{
			lower = ((parseFunc != null) ? parseFunc(text) : ((T)Convert.ChangeType(text, typeof(T))));
			hasLower = true;
		}
		if (((text2 == null || text2.Length != 0) && !(text2 == "+∞") && !(text2 == "∞")) || 1 == 0)
		{
			upper = ((parseFunc != null) ? parseFunc(text2) : ((T)Convert.ChangeType(text2, typeof(T))));
			hasUpper = true;
		}
		return new ValueRange<T>(lower, upper, flag, flag2, hasLower, hasUpper);
	}

	public override string ToString()
	{
		return ((HasLower && IsLowerInclusive) ? "[" : "(") + (HasLower ? Lower.ToString() : "-∞") + ", " + (HasUpper ? Upper.ToString() : "+∞") + ((HasUpper && IsUpperInclusive) ? "]" : ")");
	}

	public bool Contains(T value)
	{
		T val;
		if (HasLower)
		{
			ref T reference = ref value;
			val = default(T);
			if (val == null)
			{
				val = reference;
				reference = ref val;
			}
			int num = reference.CompareTo(Lower);
			if (num < 0)
			{
				return false;
			}
			if (num == 0 && !IsLowerInclusive)
			{
				return false;
			}
		}
		if (HasUpper)
		{
			ref T reference2 = ref value;
			val = default(T);
			if (val == null)
			{
				val = reference2;
				reference2 = ref val;
			}
			int num2 = reference2.CompareTo(Upper);
			if (num2 > 0)
			{
				return false;
			}
			if (num2 == 0 && !IsUpperInclusive)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEmpty()
	{
		if (!HasLower || !HasUpper)
		{
			return false;
		}
		int num = Lower.CompareTo(Upper);
		if (num <= 0)
		{
			if (num == 0)
			{
				if (IsLowerInclusive)
				{
					return !IsUpperInclusive;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public ValueRange<T> Intersect(ValueRange<T> other)
	{
		T lower = default(T);
		T upper = default(T);
		bool isLowerInclusive = true;
		bool isUpperInclusive = true;
		if (HasLower && other.HasLower)
		{
			int num = Lower.CompareTo(other.Lower);
			if (num <= 0)
			{
				if (num < 0)
				{
					lower = other.Lower;
					isLowerInclusive = other.IsLowerInclusive;
				}
				else
				{
					lower = Lower;
					isLowerInclusive = IsLowerInclusive && other.IsLowerInclusive;
				}
			}
			else
			{
				lower = Lower;
				isLowerInclusive = IsLowerInclusive;
			}
		}
		else if (HasLower)
		{
			lower = Lower;
			isLowerInclusive = IsLowerInclusive;
		}
		else if (other.HasLower)
		{
			lower = other.Lower;
			isLowerInclusive = other.IsLowerInclusive;
		}
		if (HasUpper && other.HasUpper)
		{
			int num = Upper.CompareTo(other.Upper);
			if (num >= 0)
			{
				if (num > 0)
				{
					upper = other.Upper;
					isUpperInclusive = other.IsUpperInclusive;
				}
				else
				{
					upper = Upper;
					isUpperInclusive = IsUpperInclusive && other.IsUpperInclusive;
				}
			}
			else
			{
				upper = Upper;
				isUpperInclusive = IsUpperInclusive;
			}
		}
		else if (HasUpper)
		{
			upper = Upper;
			isUpperInclusive = IsUpperInclusive;
		}
		else if (other.HasUpper)
		{
			upper = other.Upper;
			isUpperInclusive = other.IsUpperInclusive;
		}
		bool hasLower = HasLower || other.HasLower;
		bool hasUpper = HasUpper || other.HasUpper;
		return new ValueRange<T>(lower, upper, isLowerInclusive, isUpperInclusive, hasLower, hasUpper);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ValueRange<T>);
	}

	public bool Equals(ValueRange<T>? other)
	{
		if ((object)other != null && EqualityComparer<T>.Default.Equals(Lower, other.Lower) && EqualityComparer<T>.Default.Equals(Upper, other.Upper) && HasLower == other.HasLower && HasUpper == other.HasUpper && IsLowerInclusive == other.IsLowerInclusive)
		{
			return IsUpperInclusive == other.IsUpperInclusive;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((169504101 * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Lower)) * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Upper)) * -1521134295 + HasLower.GetHashCode()) * -1521134295 + HasUpper.GetHashCode()) * -1521134295 + IsLowerInclusive.GetHashCode()) * -1521134295 + IsUpperInclusive.GetHashCode();
	}

	public static bool operator ==(ValueRange<T>? left, ValueRange<T>? right)
	{
		return EqualityComparer<ValueRange<T>>.Default.Equals(left, right);
	}

	public static bool operator !=(ValueRange<T>? left, ValueRange<T>? right)
	{
		return !(left == right);
	}
}
