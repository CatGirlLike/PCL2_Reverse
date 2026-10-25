using System.Collections.Generic;
using System.Net.Http.Headers;

namespace System.Net.Http.Formatting;

internal class MediaTypeWithQualityHeaderValueComparer : IComparer<MediaTypeWithQualityHeaderValue>
{
	private static readonly MediaTypeWithQualityHeaderValueComparer _mediaTypeComparer = new MediaTypeWithQualityHeaderValueComparer();

	public static MediaTypeWithQualityHeaderValueComparer QualityComparer => _mediaTypeComparer;

	private MediaTypeWithQualityHeaderValueComparer()
	{
	}

	public int Compare(MediaTypeWithQualityHeaderValue mediaType1, MediaTypeWithQualityHeaderValue mediaType2)
	{
		if (mediaType1 == mediaType2)
		{
			return 0;
		}
		int num = CompareBasedOnQualityFactor(mediaType1, mediaType2);
		if (num == 0)
		{
			ParsedMediaTypeHeaderValue parsedMediaTypeHeaderValue = new ParsedMediaTypeHeaderValue(mediaType1);
			ParsedMediaTypeHeaderValue other = new ParsedMediaTypeHeaderValue(mediaType2);
			if (!parsedMediaTypeHeaderValue.TypesEqual(ref other))
			{
				if (parsedMediaTypeHeaderValue.IsAllMediaRange)
				{
					return -1;
				}
				if (other.IsAllMediaRange)
				{
					return 1;
				}
				if (parsedMediaTypeHeaderValue.IsSubtypeMediaRange && !other.IsSubtypeMediaRange)
				{
					return -1;
				}
				if (!parsedMediaTypeHeaderValue.IsSubtypeMediaRange && other.IsSubtypeMediaRange)
				{
					return 1;
				}
			}
			else if (!parsedMediaTypeHeaderValue.SubTypesEqual(ref other))
			{
				if (parsedMediaTypeHeaderValue.IsSubtypeMediaRange)
				{
					return -1;
				}
				if (other.IsSubtypeMediaRange)
				{
					return 1;
				}
			}
		}
		return num;
	}

	private static int CompareBasedOnQualityFactor(MediaTypeWithQualityHeaderValue mediaType1, MediaTypeWithQualityHeaderValue mediaType2)
	{
		double num = mediaType1.Quality ?? 1.0;
		double num2 = mediaType2.Quality ?? 1.0;
		double num3 = num - num2;
		if (num3 < 0.0)
		{
			return -1;
		}
		if (num3 > 0.0)
		{
			return 1;
		}
		return 0;
	}
}
