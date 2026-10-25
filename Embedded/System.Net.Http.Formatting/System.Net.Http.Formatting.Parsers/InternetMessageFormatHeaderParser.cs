using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace System.Net.Http.Formatting.Parsers;

internal class InternetMessageFormatHeaderParser
{
	private enum HeaderFieldState
	{
		Name,
		Value,
		AfterCarriageReturn,
		FoldingLine
	}

	private class CurrentHeaderFieldStore
	{
		private const int DefaultFieldNameAllocation = 128;

		private const int DefaultFieldValueAllocation = 2048;

		private static readonly char[] _linearWhiteSpace = new char[2] { ' ', '\t' };

		private readonly StringBuilder _name = new StringBuilder(128);

		private readonly StringBuilder _value = new StringBuilder(2048);

		public StringBuilder Name => _name;

		public StringBuilder Value => _value;

		public void CopyTo(HttpHeaders headers, bool ignoreHeaderValidation)
		{
			string text = _name.ToString();
			string value = _value.ToString().Trim(_linearWhiteSpace);
			if (string.Equals("expires", text, StringComparison.OrdinalIgnoreCase))
			{
				ignoreHeaderValidation = true;
			}
			if (ignoreHeaderValidation)
			{
				headers.TryAddWithoutValidation(text, value);
			}
			else
			{
				headers.Add(text, value);
			}
			Clear();
		}

		public bool IsEmpty()
		{
			if (_name.Length == 0)
			{
				return _value.Length == 0;
			}
			return false;
		}

		private void Clear()
		{
			_name.Clear();
			_value.Clear();
		}
	}

	internal const int MinHeaderSize = 2;

	private int _totalBytesConsumed;

	private int _maxHeaderSize;

	private HeaderFieldState _headerState;

	private HttpHeaders _headers;

	private CurrentHeaderFieldStore _currentHeader;

	private readonly bool _ignoreHeaderValidation;

	public InternetMessageFormatHeaderParser(HttpHeaders headers, int maxHeaderSize)
		: this(headers, maxHeaderSize, ignoreHeaderValidation: false)
	{
	}

	public InternetMessageFormatHeaderParser(HttpHeaders headers, int maxHeaderSize, bool ignoreHeaderValidation)
	{
		if (maxHeaderSize < 2)
		{
			throw Error.ArgumentMustBeGreaterThanOrEqualTo("maxHeaderSize", maxHeaderSize, 2);
		}
		if (headers == null)
		{
			throw Error.ArgumentNull("headers");
		}
		_headers = headers;
		_maxHeaderSize = maxHeaderSize;
		_ignoreHeaderValidation = ignoreHeaderValidation;
		_currentHeader = new CurrentHeaderFieldStore();
	}

	public ParserState ParseBuffer(byte[] buffer, int bytesReady, ref int bytesConsumed)
	{
		if (buffer == null)
		{
			throw Error.ArgumentNull("buffer");
		}
		ParserState result = ParserState.NeedMoreData;
		if (bytesConsumed >= bytesReady)
		{
			return result;
		}
		try
		{
			return ParseHeaderFields(buffer, bytesReady, ref bytesConsumed, ref _headerState, _maxHeaderSize, ref _totalBytesConsumed, _currentHeader, _headers, _ignoreHeaderValidation);
		}
		catch (Exception)
		{
			return ParserState.Invalid;
		}
	}

	private static ParserState ParseHeaderFields(byte[] buffer, int bytesReady, ref int bytesConsumed, ref HeaderFieldState requestHeaderState, int maximumHeaderLength, ref int totalBytesConsumed, CurrentHeaderFieldStore currentField, HttpHeaders headers, bool ignoreHeaderValidation)
	{
		int num = bytesConsumed;
		ParserState result = ParserState.DataTooBig;
		int num2 = ((maximumHeaderLength <= 0) ? int.MaxValue : (maximumHeaderLength - totalBytesConsumed + num));
		if (bytesReady < num2)
		{
			result = ParserState.NeedMoreData;
			num2 = bytesReady;
		}
		int num3;
		string string3;
		string string4;
		switch (requestHeaderState)
		{
		case HeaderFieldState.Name:
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 58)
			{
				if (buffer[bytesConsumed] == 13)
				{
					goto IL_004f;
				}
				if (++bytesConsumed != num2)
				{
					continue;
				}
				goto IL_0088;
			}
			if (bytesConsumed > num3)
			{
				string @string = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
				currentField.Name.Append(@string);
			}
			requestHeaderState = HeaderFieldState.Value;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case HeaderFieldState.Value;
		case HeaderFieldState.Value:
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 13)
			{
				if (++bytesConsumed != num2)
				{
					continue;
				}
				goto IL_0105;
			}
			if (bytesConsumed > num3)
			{
				string string2 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
				currentField.Value.Append(string2);
			}
			requestHeaderState = HeaderFieldState.AfterCarriageReturn;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case HeaderFieldState.AfterCarriageReturn;
		case HeaderFieldState.AfterCarriageReturn:
			if (buffer[bytesConsumed] != 10)
			{
				result = ParserState.Invalid;
				break;
			}
			if (currentField.IsEmpty())
			{
				result = ParserState.Done;
				bytesConsumed++;
				break;
			}
			requestHeaderState = HeaderFieldState.FoldingLine;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case HeaderFieldState.FoldingLine;
		case HeaderFieldState.FoldingLine:
			{
				if (buffer[bytesConsumed] != 32 && buffer[bytesConsumed] != 9)
				{
					currentField.CopyTo(headers, ignoreHeaderValidation);
					requestHeaderState = HeaderFieldState.Name;
					if (bytesConsumed == num2)
					{
						break;
					}
					goto case HeaderFieldState.Name;
				}
				currentField.Value.Append(' ');
				requestHeaderState = HeaderFieldState.Value;
				if (++bytesConsumed == num2)
				{
					break;
				}
				goto case HeaderFieldState.Value;
			}
			IL_0088:
			string3 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
			currentField.Name.Append(string3);
			break;
			IL_004f:
			if (!currentField.IsEmpty())
			{
				result = ParserState.Invalid;
				break;
			}
			requestHeaderState = HeaderFieldState.AfterCarriageReturn;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case HeaderFieldState.AfterCarriageReturn;
			IL_0105:
			string4 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
			currentField.Value.Append(string4);
			break;
		}
		totalBytesConsumed += bytesConsumed - num;
		return result;
	}
}
