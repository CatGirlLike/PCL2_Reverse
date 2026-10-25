using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;

namespace System.Net.Http.Formatting;

internal static class MediaTypeHeaderValueExtensions
{
	public static bool IsSubsetOf(this MediaTypeHeaderValue mediaType1, MediaTypeHeaderValue mediaType2)
	{
		MediaTypeHeaderValueRange mediaType2Range;
		return mediaType1.IsSubsetOf(mediaType2, out mediaType2Range);
	}

	public static bool IsSubsetOf(this MediaTypeHeaderValue mediaType1, MediaTypeHeaderValue mediaType2, out MediaTypeHeaderValueRange mediaType2Range)
	{
		if (mediaType2 == null)
		{
			mediaType2Range = MediaTypeHeaderValueRange.None;
			return false;
		}
		ParsedMediaTypeHeaderValue parsedMediaTypeHeaderValue = new ParsedMediaTypeHeaderValue(mediaType1);
		ParsedMediaTypeHeaderValue other = new ParsedMediaTypeHeaderValue(mediaType2);
		mediaType2Range = (other.IsAllMediaRange ? MediaTypeHeaderValueRange.AllMediaRange : (other.IsSubtypeMediaRange ? MediaTypeHeaderValueRange.SubtypeMediaRange : MediaTypeHeaderValueRange.None));
		if (!parsedMediaTypeHeaderValue.TypesEqual(ref other))
		{
			if (mediaType2Range != MediaTypeHeaderValueRange.AllMediaRange)
			{
				return false;
			}
		}
		else if (!parsedMediaTypeHeaderValue.SubTypesEqual(ref other) && mediaType2Range != MediaTypeHeaderValueRange.SubtypeMediaRange)
		{
			return false;
		}
		Collection<NameValueHeaderValue> collection = mediaType1.Parameters.AsCollection();
		int count = collection.Count;
		Collection<NameValueHeaderValue> collection2 = mediaType2.Parameters.AsCollection();
		int count2 = collection2.Count;
		for (int i = 0; i < count; i++)
		{
			NameValueHeaderValue nameValueHeaderValue = collection[i];
			bool flag = false;
			for (int j = 0; j < count2; j++)
			{
				NameValueHeaderValue obj = collection2[j];
				if (nameValueHeaderValue.Equals(obj))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}
}
