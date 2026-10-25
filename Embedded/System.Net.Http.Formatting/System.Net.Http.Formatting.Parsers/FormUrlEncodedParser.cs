using System.Collections.Generic;
using System.Text;
using System.Web.Http;

namespace System.Net.Http.Formatting.Parsers;

internal class FormUrlEncodedParser
{
	private enum NameValueState
	{
		Name,
		Value
	}

	private class CurrentNameValuePair
	{
		private const int DefaultNameAllocation = 128;

		private const int DefaultValueAllocation = 2048;

		private readonly StringBuilder _name = new StringBuilder(128);

		private readonly StringBuilder _value = new StringBuilder(2048);

		public StringBuilder Name => _name;

		public StringBuilder Value => _value;

		public void CopyTo(ICollection<KeyValuePair<string, string>> nameValuePairs)
		{
			string key = UriQueryUtility.UrlDecode(_name.ToString());
			string value = UriQueryUtility.UrlDecode(_value.ToString());
			nameValuePairs.Add(new KeyValuePair<string, string>(key, value));
			Clear();
		}

		public void CopyNameOnlyTo(ICollection<KeyValuePair<string, string>> nameValuePairs)
		{
			string key = UriQueryUtility.UrlDecode(_name.ToString());
			string empty = string.Empty;
			nameValuePairs.Add(new KeyValuePair<string, string>(key, empty));
			Clear();
		}

		private void Clear()
		{
			_name.Clear();
			_value.Clear();
		}
	}

	private const int MinMessageSize = 1;

	private long _totalBytesConsumed;

	private long _maxMessageSize;

	private NameValueState _nameValueState;

	private ICollection<KeyValuePair<string, string>> _nameValuePairs;

	private readonly CurrentNameValuePair _currentNameValuePair;

	public FormUrlEncodedParser(ICollection<KeyValuePair<string, string>> nameValuePairs, long maxMessageSize)
	{
		if (maxMessageSize < 1)
		{
			throw Error.ArgumentMustBeGreaterThanOrEqualTo("maxMessageSize", maxMessageSize, 1);
		}
		if (nameValuePairs == null)
		{
			throw Error.ArgumentNull("nameValuePairs");
		}
		_nameValuePairs = nameValuePairs;
		_maxMessageSize = maxMessageSize;
		_currentNameValuePair = new CurrentNameValuePair();
	}

	public ParserState ParseBuffer(byte[] buffer, int bytesReady, ref int bytesConsumed, bool isFinal)
	{
		if (buffer == null)
		{
			throw Error.ArgumentNull("buffer");
		}
		ParserState parserState = ParserState.NeedMoreData;
		if (bytesConsumed >= bytesReady)
		{
			if (isFinal)
			{
				parserState = CopyCurrent(parserState);
			}
			return parserState;
		}
		try
		{
			parserState = ParseNameValuePairs(buffer, bytesReady, ref bytesConsumed, ref _nameValueState, _maxMessageSize, ref _totalBytesConsumed, _currentNameValuePair, _nameValuePairs);
			if (isFinal)
			{
				parserState = CopyCurrent(parserState);
			}
		}
		catch (Exception)
		{
			parserState = ParserState.Invalid;
		}
		return parserState;
	}

	private static ParserState ParseNameValuePairs(byte[] buffer, int bytesReady, ref int bytesConsumed, ref NameValueState nameValueState, long maximumLength, ref long totalBytesConsumed, CurrentNameValuePair currentNameValuePair, ICollection<KeyValuePair<string, string>> nameValuePairs)
	{
		int num = bytesConsumed;
		ParserState result = ParserState.DataTooBig;
		long num2 = ((maximumLength <= 0) ? long.MaxValue : (maximumLength - totalBytesConsumed + num));
		if (bytesReady < num2)
		{
			result = ParserState.NeedMoreData;
			num2 = bytesReady;
		}
		NameValueState nameValueState2 = nameValueState;
		if (nameValueState2 == NameValueState.Name)
		{
			goto IL_003f;
		}
		if (nameValueState2 == NameValueState.Value)
		{
			goto IL_00f1;
		}
		goto IL_0176;
		IL_00b8:
		nameValueState = NameValueState.Value;
		if (++bytesConsumed != num2)
		{
			goto IL_00f1;
		}
		goto IL_0176;
		IL_00f1:
		int num3 = bytesConsumed;
		while (buffer[bytesConsumed] != 38)
		{
			if (++bytesConsumed != num2)
			{
				continue;
			}
			goto IL_0106;
		}
		if (bytesConsumed > num3)
		{
			string @string = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
			currentNameValuePair.Value.Append(@string);
		}
		currentNameValuePair.CopyTo(nameValuePairs);
		nameValueState = NameValueState.Name;
		if (++bytesConsumed != num2)
		{
			goto IL_003f;
		}
		goto IL_0176;
		IL_003f:
		while (true)
		{
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 61 && buffer[bytesConsumed] != 38)
			{
				if (++bytesConsumed != num2)
				{
					continue;
				}
				goto IL_0054;
			}
			if (bytesConsumed > num3)
			{
				string string2 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
				currentNameValuePair.Name.Append(string2);
			}
			if (buffer[bytesConsumed] != 61)
			{
				currentNameValuePair.CopyNameOnlyTo(nameValuePairs);
				if (++bytesConsumed == num2)
				{
					break;
				}
				continue;
			}
			goto IL_00b8;
			IL_0054:
			string string3 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
			currentNameValuePair.Name.Append(string3);
			break;
		}
		goto IL_0176;
		IL_0106:
		string string4 = Encoding.UTF8.GetString(buffer, num3, bytesConsumed - num3);
		currentNameValuePair.Value.Append(string4);
		goto IL_0176;
		IL_0176:
		totalBytesConsumed += bytesConsumed - num;
		return result;
	}

	private ParserState CopyCurrent(ParserState parseState)
	{
		if (_nameValueState == NameValueState.Name)
		{
			if (_totalBytesConsumed > 0)
			{
				_currentNameValuePair.CopyNameOnlyTo(_nameValuePairs);
			}
		}
		else
		{
			_currentNameValuePair.CopyTo(_nameValuePairs);
		}
		if (parseState != 0)
		{
			return parseState;
		}
		return ParserState.Done;
	}
}
