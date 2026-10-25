using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Properties;
using System.Text;
using System.Web.Http;

namespace System.Net.Http.Formatting.Parsers;

internal class MimeMultipartParser
{
	private enum BodyPartState
	{
		BodyPart,
		AfterFirstCarriageReturn,
		AfterFirstLineFeed,
		AfterFirstDash,
		Boundary,
		AfterBoundary,
		AfterSecondDash,
		AfterSecondCarriageReturn
	}

	private enum MessageState
	{
		Boundary,
		BodyPart,
		CloseDelimiter
	}

	public enum State
	{
		NeedMoreData,
		BodyPartCompleted,
		Invalid,
		DataTooBig
	}

	[DebuggerDisplay("{DebuggerToString()}")]
	private class CurrentBodyPartStore
	{
		private const int InitialOffset = 2;

		private byte[] _boundaryStore = new byte[256];

		private int _boundaryStoreLength;

		private byte[] _referenceBoundary = new byte[256];

		private int _referenceBoundaryLength;

		private byte[] _boundary = new byte[256];

		private int _boundaryLength;

		private ArraySegment<byte> _bodyPart = _emptyBodyPart;

		private bool _isFinal;

		private bool _isFirst = true;

		private bool _releaseDiscardedBoundary;

		private int _boundaryOffset;

		public bool HasPotentialBoundaryLeftOver { get; set; }

		public int BoundaryDelta
		{
			get
			{
				if (_boundaryLength - _boundaryOffset <= 0)
				{
					return _boundaryLength;
				}
				return _boundaryLength - _boundaryOffset;
			}
		}

		public ArraySegment<byte> BodyPart
		{
			get
			{
				return _bodyPart;
			}
			set
			{
				_bodyPart = value;
			}
		}

		public bool IsFinal => _isFinal;

		public CurrentBodyPartStore(string referenceBoundary)
		{
			_referenceBoundary[0] = 13;
			_referenceBoundary[1] = 10;
			_referenceBoundary[2] = 45;
			_referenceBoundary[3] = 45;
			_referenceBoundaryLength = 4 + Encoding.UTF8.GetBytes(referenceBoundary, 0, referenceBoundary.Length, _referenceBoundary, 4);
			_boundary[0] = 13;
			_boundary[1] = 10;
			_boundaryLength = 2;
		}

		public void ResetBoundaryOffset()
		{
			_boundaryOffset = _boundaryLength;
		}

		public void ResetBoundary()
		{
			if (HasPotentialBoundaryLeftOver)
			{
				Buffer.BlockCopy(_boundary, 0, _boundaryStore, 0, _boundaryOffset);
				_boundaryStoreLength = _boundaryOffset;
				HasPotentialBoundaryLeftOver = false;
				_releaseDiscardedBoundary = true;
			}
			_boundaryLength = 0;
			_boundaryOffset = 0;
		}

		public void AppendBoundary(byte data)
		{
			_boundary[_boundaryLength++] = data;
		}

		public bool AppendBoundary(byte[] data, int offset, int count)
		{
			if (_boundaryLength + count > _referenceBoundaryLength + 6)
			{
				return false;
			}
			int i = _boundaryLength;
			Buffer.BlockCopy(data, offset, _boundary, _boundaryLength, count);
			_boundaryLength += count;
			for (int num = Math.Min(_boundaryLength, _referenceBoundaryLength); i < num; i++)
			{
				if (_boundary[i] != _referenceBoundary[i])
				{
					return false;
				}
			}
			return true;
		}

		public ArraySegment<byte> GetDiscardedBoundary()
		{
			if (_boundaryStoreLength > 0 && _releaseDiscardedBoundary)
			{
				ArraySegment<byte> result = new ArraySegment<byte>(_boundaryStore, 0, _boundaryStoreLength);
				_boundaryStoreLength = 0;
				return result;
			}
			return _emptyBodyPart;
		}

		public bool IsBoundaryValid()
		{
			int num = 0;
			if (_isFirst)
			{
				num = 2;
			}
			int i;
			for (i = num; i < _referenceBoundaryLength; i++)
			{
				if (_boundary[i] != _referenceBoundary[i])
				{
					return false;
				}
			}
			bool isFinal = false;
			if (_boundary[i] == 45 && _boundary[i + 1] == 45)
			{
				isFinal = true;
				i += 2;
			}
			for (; i < _boundaryLength - 2; i++)
			{
				if (_boundary[i] != 32 && _boundary[i] != 9)
				{
					return false;
				}
			}
			_isFinal = isFinal;
			_isFirst = false;
			return true;
		}

		public bool IsBoundaryComplete()
		{
			if (!IsBoundaryValid())
			{
				return false;
			}
			if (_boundaryLength < _referenceBoundaryLength)
			{
				return false;
			}
			if (_boundaryLength == _referenceBoundaryLength + 1 && _boundary[_referenceBoundaryLength] == 45)
			{
				return false;
			}
			return true;
		}

		public void ClearBodyPart()
		{
			BodyPart = _emptyBodyPart;
		}

		public void ClearAll()
		{
			_releaseDiscardedBoundary = false;
			HasPotentialBoundaryLeftOver = false;
			_boundaryLength = 0;
			_boundaryOffset = 0;
			_boundaryStoreLength = 0;
			_isFinal = false;
			ClearBodyPart();
		}

		private string DebuggerToString()
		{
			string @string = Encoding.UTF8.GetString(_referenceBoundary, 0, _referenceBoundaryLength);
			string string2 = Encoding.UTF8.GetString(_boundary, 0, _boundaryLength);
			return string.Format(CultureInfo.InvariantCulture, "Expected: {0} *** Current: {1}", new object[2] { @string, string2 });
		}
	}

	internal const int MinMessageSize = 10;

	private const int MaxBoundarySize = 256;

	private const byte HTAB = 9;

	private const byte SP = 32;

	private const byte CR = 13;

	private const byte LF = 10;

	private const byte Dash = 45;

	private static readonly ArraySegment<byte> _emptyBodyPart = new ArraySegment<byte>(new byte[0]);

	private long _totalBytesConsumed;

	private long _maxMessageSize;

	private BodyPartState _bodyPartState;

	private string _boundary;

	private CurrentBodyPartStore _currentBoundary;

	public bool IsWaitingForEndOfMessage
	{
		get
		{
			if (_bodyPartState == BodyPartState.AfterBoundary && _currentBoundary != null)
			{
				return _currentBoundary.IsFinal;
			}
			return false;
		}
	}

	public MimeMultipartParser(string boundary, long maxMessageSize)
	{
		if (maxMessageSize < 10)
		{
			throw Error.ArgumentMustBeGreaterThanOrEqualTo("maxMessageSize", maxMessageSize, 10);
		}
		if (string.IsNullOrWhiteSpace(boundary))
		{
			throw Error.ArgumentNull("boundary");
		}
		if (boundary.Length > 246)
		{
			throw Error.ArgumentMustBeLessThanOrEqualTo("boundary", boundary.Length, 246);
		}
		if (boundary.EndsWith(" ", StringComparison.Ordinal))
		{
			throw Error.Argument("boundary", System.Net.Http.Properties.Resources.MimeMultipartParserBadBoundary);
		}
		_maxMessageSize = maxMessageSize;
		_boundary = boundary;
		_currentBoundary = new CurrentBodyPartStore(_boundary);
		_bodyPartState = BodyPartState.AfterFirstLineFeed;
	}

	public bool CanParseMore(int bytesRead, int bytesConsumed)
	{
		if (bytesConsumed < bytesRead)
		{
			return true;
		}
		if (bytesRead == 0 && IsWaitingForEndOfMessage)
		{
			return true;
		}
		return false;
	}

	public State ParseBuffer(byte[] buffer, int bytesReady, ref int bytesConsumed, out ArraySegment<byte> remainingBodyPart, out ArraySegment<byte> bodyPart, out bool isFinalBodyPart)
	{
		if (buffer == null)
		{
			throw Error.ArgumentNull("buffer");
		}
		State state = State.NeedMoreData;
		remainingBodyPart = _emptyBodyPart;
		bodyPart = _emptyBodyPart;
		isFinalBodyPart = false;
		try
		{
			state = ParseBodyPart(buffer, bytesReady, ref bytesConsumed, ref _bodyPartState, _maxMessageSize, ref _totalBytesConsumed, _currentBoundary);
		}
		catch (Exception)
		{
			state = State.Invalid;
		}
		remainingBodyPart = _currentBoundary.GetDiscardedBoundary();
		bodyPart = _currentBoundary.BodyPart;
		if (state == State.BodyPartCompleted)
		{
			isFinalBodyPart = _currentBoundary.IsFinal;
			_currentBoundary.ClearAll();
		}
		else
		{
			_currentBoundary.ClearBodyPart();
		}
		return state;
	}

	private static State ParseBodyPart(byte[] buffer, int bytesReady, ref int bytesConsumed, ref BodyPartState bodyPartState, long maximumMessageLength, ref long totalBytesConsumed, CurrentBodyPartStore currentBodyPart)
	{
		int num = bytesConsumed;
		if (bytesReady == 0 && bodyPartState == BodyPartState.AfterBoundary && currentBodyPart.IsFinal)
		{
			return State.BodyPartCompleted;
		}
		State state = State.DataTooBig;
		long num2 = ((maximumMessageLength <= 0) ? long.MaxValue : (maximumMessageLength - totalBytesConsumed + bytesConsumed));
		if (num2 == 0L)
		{
			return State.DataTooBig;
		}
		if (bytesReady <= num2)
		{
			state = State.NeedMoreData;
			num2 = bytesReady;
		}
		currentBodyPart.ResetBoundaryOffset();
		int num3;
		switch (bodyPartState)
		{
		case BodyPartState.BodyPart:
			while (buffer[bytesConsumed] != 13)
			{
				if (++bytesConsumed == num2)
				{
					goto end_IL_0050;
				}
			}
			currentBodyPart.AppendBoundary(13);
			bodyPartState = BodyPartState.AfterFirstCarriageReturn;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case BodyPartState.AfterFirstCarriageReturn;
		case BodyPartState.AfterFirstCarriageReturn:
			if (buffer[bytesConsumed] != 10)
			{
				currentBodyPart.ResetBoundary();
				bodyPartState = BodyPartState.BodyPart;
				goto case BodyPartState.BodyPart;
			}
			currentBodyPart.AppendBoundary(10);
			bodyPartState = BodyPartState.AfterFirstLineFeed;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case BodyPartState.AfterFirstLineFeed;
		case BodyPartState.AfterFirstLineFeed:
			if (buffer[bytesConsumed] == 13)
			{
				currentBodyPart.ResetBoundary();
				currentBodyPart.AppendBoundary(13);
				bodyPartState = BodyPartState.AfterFirstCarriageReturn;
				if (++bytesConsumed == num2)
				{
					break;
				}
				goto case BodyPartState.AfterFirstCarriageReturn;
			}
			if (buffer[bytesConsumed] != 45)
			{
				currentBodyPart.ResetBoundary();
				bodyPartState = BodyPartState.BodyPart;
				goto case BodyPartState.BodyPart;
			}
			currentBodyPart.AppendBoundary(45);
			bodyPartState = BodyPartState.AfterFirstDash;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case BodyPartState.AfterFirstDash;
		case BodyPartState.AfterFirstDash:
			if (buffer[bytesConsumed] != 45)
			{
				currentBodyPart.ResetBoundary();
				bodyPartState = BodyPartState.BodyPart;
				goto case BodyPartState.BodyPart;
			}
			currentBodyPart.AppendBoundary(45);
			bodyPartState = BodyPartState.Boundary;
			if (++bytesConsumed == num2)
			{
				break;
			}
			goto case BodyPartState.Boundary;
		case BodyPartState.Boundary:
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 13)
			{
				if (++bytesConsumed != num2)
				{
					continue;
				}
				goto IL_0198;
			}
			if (bytesConsumed > num3 && !currentBodyPart.AppendBoundary(buffer, num3, bytesConsumed - num3))
			{
				currentBodyPart.ResetBoundary();
				bodyPartState = BodyPartState.BodyPart;
				goto case BodyPartState.BodyPart;
			}
			goto case BodyPartState.AfterBoundary;
		case BodyPartState.AfterBoundary:
			if (buffer[bytesConsumed] == 45 && !currentBodyPart.IsFinal)
			{
				currentBodyPart.AppendBoundary(45);
				if (++bytesConsumed == num2)
				{
					bodyPartState = BodyPartState.AfterSecondDash;
					break;
				}
				goto case BodyPartState.AfterSecondDash;
			}
			num3 = bytesConsumed;
			while (buffer[bytesConsumed] != 13)
			{
				if (++bytesConsumed != num2)
				{
					continue;
				}
				goto IL_023f;
			}
			if (bytesConsumed > num3 && !currentBodyPart.AppendBoundary(buffer, num3, bytesConsumed - num3))
			{
				currentBodyPart.ResetBoundary();
				bodyPartState = BodyPartState.BodyPart;
			}
			else
			{
				if (buffer[bytesConsumed] == 13)
				{
					currentBodyPart.AppendBoundary(13);
					if (++bytesConsumed == num2)
					{
						bodyPartState = BodyPartState.AfterSecondCarriageReturn;
						break;
					}
					goto case BodyPartState.AfterSecondCarriageReturn;
				}
				currentBodyPart.ResetBoundary();
				bodyPartState = BodyPartState.BodyPart;
			}
			goto case BodyPartState.BodyPart;
		case BodyPartState.AfterSecondDash:
			if (buffer[bytesConsumed] == 45)
			{
				currentBodyPart.AppendBoundary(45);
				bytesConsumed++;
				if (currentBodyPart.IsBoundaryComplete())
				{
					bodyPartState = BodyPartState.AfterBoundary;
					state = State.NeedMoreData;
					break;
				}
				currentBodyPart.ResetBoundary();
				if (bytesConsumed == num2)
				{
					break;
				}
			}
			else
			{
				currentBodyPart.ResetBoundary();
				bodyPartState = BodyPartState.BodyPart;
			}
			goto case BodyPartState.BodyPart;
		case BodyPartState.AfterSecondCarriageReturn:
			{
				if (buffer[bytesConsumed] != 10)
				{
					currentBodyPart.ResetBoundary();
					bodyPartState = BodyPartState.BodyPart;
				}
				else
				{
					currentBodyPart.AppendBoundary(10);
					bytesConsumed++;
					bodyPartState = BodyPartState.BodyPart;
					if (currentBodyPart.IsBoundaryComplete())
					{
						state = State.BodyPartCompleted;
						break;
					}
					currentBodyPart.ResetBoundary();
					if (bytesConsumed == num2)
					{
						break;
					}
				}
				goto case BodyPartState.BodyPart;
			}
			IL_0198:
			if (currentBodyPart.AppendBoundary(buffer, num3, bytesConsumed - num3))
			{
				if (currentBodyPart.IsBoundaryComplete())
				{
					bodyPartState = BodyPartState.AfterBoundary;
				}
			}
			else
			{
				currentBodyPart.ResetBoundary();
				bodyPartState = BodyPartState.BodyPart;
			}
			break;
			IL_023f:
			if (!currentBodyPart.AppendBoundary(buffer, num3, bytesConsumed - num3))
			{
				currentBodyPart.ResetBoundary();
				bodyPartState = BodyPartState.BodyPart;
			}
			break;
			end_IL_0050:
			break;
		}
		if (num < bytesConsumed)
		{
			int boundaryDelta = currentBodyPart.BoundaryDelta;
			if (boundaryDelta > 0 && state != State.BodyPartCompleted)
			{
				currentBodyPart.HasPotentialBoundaryLeftOver = true;
			}
			int count = bytesConsumed - num - boundaryDelta;
			currentBodyPart.BodyPart = new ArraySegment<byte>(buffer, num, count);
		}
		totalBytesConsumed += bytesConsumed - num;
		return state;
	}
}
