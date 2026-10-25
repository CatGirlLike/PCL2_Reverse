using System.Net.Http.Headers;

namespace System.Net.Http.Formatting;

internal struct ParsedMediaTypeHeaderValue
{
	private const char MediaRangeAsterisk = '*';

	private const char MediaTypeSubtypeDelimiter = '/';

	private readonly string _mediaType;

	private readonly int _delimiterIndex;

	private readonly bool _isAllMediaRange;

	private readonly bool _isSubtypeMediaRange;

	public bool IsAllMediaRange => _isAllMediaRange;

	public bool IsSubtypeMediaRange => _isSubtypeMediaRange;

	public ParsedMediaTypeHeaderValue(MediaTypeHeaderValue mediaTypeHeaderValue)
	{
		string text = (_mediaType = mediaTypeHeaderValue.MediaType);
		_delimiterIndex = text.IndexOf('/');
		_isAllMediaRange = false;
		_isSubtypeMediaRange = false;
		int length = text.Length;
		if (_delimiterIndex == length - 2 && text[length - 1] == '*')
		{
			_isSubtypeMediaRange = true;
			if (_delimiterIndex == 1 && text[0] == '*')
			{
				_isAllMediaRange = true;
			}
		}
	}

	public bool TypesEqual(ref ParsedMediaTypeHeaderValue other)
	{
		if (_delimiterIndex != other._delimiterIndex)
		{
			return false;
		}
		return string.Compare(_mediaType, 0, other._mediaType, 0, _delimiterIndex, StringComparison.OrdinalIgnoreCase) == 0;
	}

	public bool SubTypesEqual(ref ParsedMediaTypeHeaderValue other)
	{
		int num = _mediaType.Length - _delimiterIndex - 1;
		if (num != other._mediaType.Length - other._delimiterIndex - 1)
		{
			return false;
		}
		return string.Compare(_mediaType, _delimiterIndex + 1, other._mediaType, other._delimiterIndex + 1, num, StringComparison.OrdinalIgnoreCase) == 0;
	}
}
