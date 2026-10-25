using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using MeloongCore.Extensions;

namespace MeloongCore;

public static class MathUtils
{
	[CompilerGenerated]
	private sealed class _003CGetPrimeFactors_003Ed__10 : IEnumerable<int>, IEnumerable, IEnumerator<int>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private int _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private int num;

		public int _003C_003E3__num;

		private int _003Ci_003E5__2;

		int IEnumerator<int>.Current
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
		public _003CGetPrimeFactors_003Ed__10(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num;
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				this.num = Math.Abs(this.num);
				goto IL_005e;
			case 1:
				_003C_003E1__state = -1;
				this.num /= 2;
				goto IL_005e;
			case 2:
				_003C_003E1__state = -1;
				this.num /= _003Ci_003E5__2;
				num = (int)Math.Sqrt(this.num);
				goto IL_00c5;
			case 3:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_005e:
				if (this.num % 2 == 0 && this.num > 1)
				{
					_003C_003E2__current = 2;
					_003C_003E1__state = 1;
					return true;
				}
				_003Ci_003E5__2 = 3;
				num = (int)Math.Sqrt(this.num);
				goto IL_00e2;
				IL_00c5:
				if (this.num % _003Ci_003E5__2 == 0)
				{
					_003C_003E2__current = _003Ci_003E5__2;
					_003C_003E1__state = 2;
					return true;
				}
				_003Ci_003E5__2 += 2;
				goto IL_00e2;
				IL_00e2:
				if (_003Ci_003E5__2 > num)
				{
					if (this.num > 2)
					{
						_003C_003E2__current = this.num;
						_003C_003E1__state = 3;
						return true;
					}
					break;
				}
				goto IL_00c5;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<int> IEnumerable<int>.GetEnumerator()
		{
			_003CGetPrimeFactors_003Ed__10 _003CGetPrimeFactors_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetPrimeFactors_003Ed__ = this;
			}
			else
			{
				_003CGetPrimeFactors_003Ed__ = new _003CGetPrimeFactors_003Ed__10(0);
			}
			_003CGetPrimeFactors_003Ed__.num = _003C_003E3__num;
			return _003CGetPrimeFactors_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<int>)this).GetEnumerator();
		}
	}

	public static string ConvertRadix(this string input, int fromRadix, int toRadix)
	{
		if (fromRadix < 2 || fromRadix > "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz/+=!?@#$%^&*()[]{}<>;:',".Length)
		{
			throw new ArgumentOutOfRangeException("fromRadix", "fromRadix must be between 2 and 86.");
		}
		if (string.IsNullOrEmpty(input))
		{
			return "0";
		}
		bool flag = input.StartsWithF("-");
		if (flag)
		{
			input = input.TrimStart(new char[1] { '-' });
		}
		BigInteger bigInteger = 0;
		string text = input;
		foreach (char c in text)
		{
			int num = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz/+=!?@#$%^&*()[]{}<>;:',".IndexOf(c);
			if (num == -1 || num >= fromRadix)
			{
				throw new ArgumentException($"Character '{c}' in input '{input}' is not a valid digit for radix {fromRadix}.");
			}
			bigInteger = bigInteger * fromRadix + num;
		}
		return (bigInteger * ((!flag) ? 1 : (-1))).ConvertRadix(toRadix);
	}

	public static string ConvertRadix(this long input, int toRadix)
	{
		return ((BigInteger)input).ConvertRadix(toRadix);
	}

	public static string ConvertRadix(this ulong input, int toRadix)
	{
		return ((BigInteger)input).ConvertRadix(toRadix);
	}

	public static string ConvertRadix(this int input, int toRadix)
	{
		return ((BigInteger)input).ConvertRadix(toRadix);
	}

	public static string ConvertRadix(this byte input, int toRadix)
	{
		return ((BigInteger)input).ConvertRadix(toRadix);
	}

	public static string ConvertRadix(this BigInteger input, int toRadix)
	{
		if (toRadix < 2 || toRadix > "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz/+=!?@#$%^&*()[]{}<>;:',".Length)
		{
			throw new ArgumentOutOfRangeException("toRadix", "toRadix must be between 2 and 86.");
		}
		if (input == 0L)
		{
			return "0";
		}
		bool flag = input < 0L;
		List<char> list = new List<char>();
		BigInteger bigInteger = BigInteger.Abs(input);
		BigInteger divisor = toRadix;
		while (bigInteger > 0L)
		{
			bigInteger = BigInteger.DivRem(bigInteger, divisor, out var remainder);
			list.Add("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz/+=!?@#$%^&*()[]{}<>;:',"[(int)remainder]);
		}
		if (flag)
		{
			list.Add('-');
		}
		list.Reverse();
		return new string(list.ToArray());
	}

	public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
	{
		if (value.CompareTo(min) < 0)
		{
			return min;
		}
		if (value.CompareTo(max) > 0)
		{
			return max;
		}
		return value;
	}

	public static double Lerp(double start, double end, double percentage)
	{
		return Math.Round(start + (end - start) * percentage, 7);
	}

	public static double Bezier(double t, double x1, double y1, double x2, double y2, double accuracy = 0.01)
	{
		if (t < 1E-07 || double.IsNaN(t))
		{
			return 0.0;
		}
		if (t > 0.9999999)
		{
			return 1.0;
		}
		double num = t;
		int num2 = 1000;
		double num3;
		do
		{
			num3 = 3.0 * num * ((0.33333333 + x1 - x2) * num * num + (x2 - 2.0 * x1) * num + x1);
			num += (t - num3) * 0.5;
			num2--;
		}
		while (!(Math.Abs(num3 - t) < accuracy) && num2 > 0);
		return 3.0 * num * ((0.33333333 + y1 - y2) * num * num + (y2 - 2.0 * y1) * num + y1);
	}

	public static bool IsNear(this double value, double target, double tolerance = 1E-06)
	{
		return Math.Abs(value - target) < tolerance;
	}

	[IteratorStateMachine(typeof(_003CGetPrimeFactors_003Ed__10))]
	public static IEnumerable<int> GetPrimeFactors(int num)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetPrimeFactors_003Ed__10(-2)
		{
			_003C_003E3__num = num
		};
	}

	public static int Gcd(int a, int b)
	{
		a = Math.Abs(a);
		b = Math.Abs(b);
		while (b != 0)
		{
			int num = b;
			b = a % b;
			a = num;
		}
		return a;
	}

	public static int Gcd(double a, int b)
	{
		return Gcd((int)Math.Round(a), b);
	}
}
