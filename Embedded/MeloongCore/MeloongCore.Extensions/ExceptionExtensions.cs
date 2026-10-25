using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MeloongCore.Extensions;

public static class ExceptionExtensions
{
	[CompilerGenerated]
	private sealed class _003CFlatten_003Ed__3 : IEnumerable<Exception>, IEnumerable, IEnumerator<Exception>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Exception _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Exception ex;

		public Exception _003C_003E3__ex;

		private Stack<Exception> _003CtoFlatten_003E5__2;

		private Exception _003Ccurrent_003E5__3;

		Exception IEnumerator<Exception>.Current
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
		public _003CFlatten_003Ed__3(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003CtoFlatten_003E5__2 = null;
			_003Ccurrent_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CtoFlatten_003E5__2 = new Stack<Exception>();
				_003CtoFlatten_003E5__2.Push(this.ex);
				break;
			case 1:
				_003C_003E1__state = -1;
				if (_003Ccurrent_003E5__3 is AggregateException ex)
				{
					foreach (Exception innerException in ex.InnerExceptions)
					{
						_003CtoFlatten_003E5__2.Push(innerException);
					}
				}
				else if (_003Ccurrent_003E5__3.InnerException != null)
				{
					_003CtoFlatten_003E5__2.Push(_003Ccurrent_003E5__3.InnerException);
				}
				_003Ccurrent_003E5__3 = null;
				break;
			}
			if (_003CtoFlatten_003E5__2.Any())
			{
				_003Ccurrent_003E5__3 = _003CtoFlatten_003E5__2.Pop();
				_003C_003E2__current = _003Ccurrent_003E5__3;
				_003C_003E1__state = 1;
				return true;
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
		IEnumerator<Exception> IEnumerable<Exception>.GetEnumerator()
		{
			_003CFlatten_003Ed__3 _003CFlatten_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CFlatten_003Ed__ = this;
			}
			else
			{
				_003CFlatten_003Ed__ = new _003CFlatten_003Ed__3(0);
			}
			_003CFlatten_003Ed__.ex = _003C_003E3__ex;
			return _003CFlatten_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Exception>)this).GetEnumerator();
		}
	}

	public static Exception RootException(this Exception ex)
	{
		while (true)
		{
			Exception innerException = ex.InnerException;
			if (innerException == null)
			{
				break;
			}
			ex = innerException;
		}
		return ex;
	}

	public static bool IsCanceled(this Exception ex)
	{
		foreach (Exception item in ex.Flatten())
		{
			if ((item is OperationCanceledException || item is ThreadInterruptedException) ? true : false)
			{
				return true;
			}
		}
		return false;
	}

	public static void ThrowIfCanceled(this Exception ex)
	{
		foreach (Exception item in ex.Flatten())
		{
			if ((item is OperationCanceledException || item is ThreadInterruptedException) ? true : false)
			{
				throw item;
			}
		}
	}

	[IteratorStateMachine(typeof(_003CFlatten_003Ed__3))]
	public static IEnumerable<Exception> Flatten(this Exception ex)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFlatten_003Ed__3(-2)
		{
			_003C_003E3__ex = ex
		};
	}

	public static string GetDisplay(this Exception? ex, bool multiline)
	{
		if (ex == null)
		{
			return "无可用错误信息！";
		}
		List<string> list = new List<string>();
		bool flag = false;
		foreach (Exception item in ex.Flatten())
		{
			if (multiline)
			{
				list.Add((flag ? "→ " : "") + getExceptionMessage(item).ReplaceLineEndings("\r\n", mergeMultiple: true));
				IEnumerable<string> collection = (item.StackTrace?.SplitLines(skipEmptyLines: true) ?? Array.Empty<string>()).Select((string l) => l.BeforeLast("(") + l.AfterLast(")")).Distinct();
				list.AddRange(collection);
				if (item.GetType() != typeof(Exception))
				{
					list.Add("   错误类型：" + item.GetType().FullName);
				}
			}
			else
			{
				list.Add(getExceptionMessage(item).ReplaceLineEndings(" ", mergeMultiple: true));
			}
			flag = true;
		}
		string text = null;
		Exception ex2 = ex.RootException();
		if ((ex2 is TypeLoadException || ex2 is BadImageFormatException || ex2 is MissingMethodException || ex2 is NotImplementedException || ex2 is TypeInitializationException) ? true : false)
		{
			text = "运行环境存在问题。请尝试重新安装 .NET Framework 4.8 然后再试。若无法安装，请先卸载较新版本的 .NET Framework，然后再尝试安装。";
		}
		else if (ex2 is UnauthorizedAccessException)
		{
			text = "程序权限不足。请尝试右键程序，选择以管理员身份运行，或将文件移动到其他文件夹。";
		}
		else if (ex2 is OutOfMemoryException)
		{
			text = "系统的运行内存不足。请在关闭一部分不需要的程序后再试。";
		}
		else if (ex2 is SocketException && list.Any((string l) => l.Contains("WSAStartup")))
		{
			text = "请尝试卸载中国移动云盘，然后再试。";
		}
		else if (ex.IsBadNetwork())
		{
			text = "你的网络环境不佳，请稍后再试，或使用 VPN 改善网络环境。";
		}
		if (multiline)
		{
			if (text == null)
			{
				return list.Join("\r\n");
			}
			return text + "\r\n\r\n——————— 详细错误信息 ———————\r\n" + list.Join("\r\n");
		}
		list = list.Distinct().ToList();
		if (text == null)
		{
			list.Reverse();
			return list.Join(" ← ");
		}
		return text + "（" + list.First() + "）";
		static string getExceptionMessage(Exception e)
		{
			string message = e.Message;
			string text2 = ((e is WebException ex3) ? $" (Status={ex3.Status})" : ((!(e is SocketException ex4)) ? "" : $" (SocketErrorCode={ex4.SocketErrorCode})"));
			return message + text2;
		}
	}

	public static bool IsBadNetwork(this Exception ex)
	{
		foreach (Exception item in ex.Flatten())
		{
			if (item is TimeoutException)
			{
				return true;
			}
			if (item is TaskCanceledException)
			{
				return true;
			}
			if (item is HttpRequestCodeException ex2)
			{
				if (ex2.StatusCode == HttpStatusCode.Forbidden)
				{
					return false;
				}
				if (ex2.StatusCode == HttpStatusCode.RequestTimeout)
				{
					return true;
				}
			}
			else if (item is WebException ex3)
			{
				if (ex3.Response is HttpWebResponse httpWebResponse)
				{
					if (httpWebResponse.StatusCode == HttpStatusCode.Forbidden)
					{
						return false;
					}
					if (httpWebResponse.StatusCode == HttpStatusCode.RequestTimeout)
					{
						return true;
					}
				}
				bool flag;
				switch (ex3.Status)
				{
				case WebExceptionStatus.NameResolutionFailure:
				case WebExceptionStatus.ConnectFailure:
				case WebExceptionStatus.ReceiveFailure:
				case WebExceptionStatus.SendFailure:
				case WebExceptionStatus.ConnectionClosed:
				case WebExceptionStatus.SecureChannelFailure:
				case WebExceptionStatus.KeepAliveFailure:
				case WebExceptionStatus.Timeout:
				case WebExceptionStatus.ProxyNameResolutionFailure:
					flag = true;
					break;
				default:
					flag = false;
					break;
				}
				if (flag)
				{
					return true;
				}
			}
			else if (item is SocketException { SocketErrorCode: var socketErrorCode })
			{
				bool flag;
				switch (socketErrorCode)
				{
				case SocketError.NetworkDown:
				case SocketError.NetworkUnreachable:
				case SocketError.NetworkReset:
				case SocketError.ConnectionAborted:
				case SocketError.ConnectionReset:
				case SocketError.TimedOut:
				case SocketError.ConnectionRefused:
				case SocketError.HostUnreachable:
				case SocketError.HostNotFound:
				case SocketError.TryAgain:
				case SocketError.NoData:
					flag = true;
					break;
				default:
					flag = false;
					break;
				}
				if (flag)
				{
					return true;
				}
			}
		}
		return false;
	}
}
