using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace System.Net.Http.Formatting;

public class DefaultContentNegotiator : IContentNegotiator
{
	public bool ExcludeMatchOnTypeOnly { get; private set; }

	public DefaultContentNegotiator()
		: this(excludeMatchOnTypeOnly: false)
	{
	}

	public DefaultContentNegotiator(bool excludeMatchOnTypeOnly)
	{
		ExcludeMatchOnTypeOnly = excludeMatchOnTypeOnly;
	}

	public virtual ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (request == null)
		{
			throw Error.ArgumentNull("request");
		}
		if (formatters == null)
		{
			throw Error.ArgumentNull("formatters");
		}
		Collection<MediaTypeFormatterMatch> matches = ComputeFormatterMatches(type, request, formatters);
		MediaTypeFormatterMatch mediaTypeFormatterMatch = SelectResponseMediaTypeFormatter(matches);
		if (mediaTypeFormatterMatch != null)
		{
			Encoding encoding = SelectResponseCharacterEncoding(request, mediaTypeFormatterMatch.Formatter);
			if (encoding != null)
			{
				mediaTypeFormatterMatch.MediaType.CharSet = encoding.WebName;
			}
			MediaTypeHeaderValue mediaType = mediaTypeFormatterMatch.MediaType;
			return new ContentNegotiationResult(mediaTypeFormatterMatch.Formatter.GetPerRequestFormatterInstance(type, request, mediaType), mediaType);
		}
		return null;
	}

	protected virtual Collection<MediaTypeFormatterMatch> ComputeFormatterMatches(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (request == null)
		{
			throw Error.ArgumentNull("request");
		}
		if (formatters == null)
		{
			throw Error.ArgumentNull("formatters");
		}
		IEnumerable<MediaTypeWithQualityHeaderValue> enumerable = null;
		ListWrapperCollection<MediaTypeFormatterMatch> listWrapperCollection = new ListWrapperCollection<MediaTypeFormatterMatch>();
		MediaTypeFormatter[] writingFormatters = GetWritingFormatters(formatters);
		foreach (MediaTypeFormatter mediaTypeFormatter in writingFormatters)
		{
			MediaTypeFormatterMatch mediaTypeFormatterMatch = null;
			if (!mediaTypeFormatter.CanWriteType(type))
			{
				continue;
			}
			if ((mediaTypeFormatterMatch = MatchMediaTypeMapping(request, mediaTypeFormatter)) != null)
			{
				listWrapperCollection.Add(mediaTypeFormatterMatch);
				continue;
			}
			if (enumerable == null)
			{
				enumerable = SortMediaTypeWithQualityHeaderValuesByQFactor(request.Headers.Accept);
			}
			if ((mediaTypeFormatterMatch = MatchAcceptHeader(enumerable, mediaTypeFormatter)) != null)
			{
				listWrapperCollection.Add(mediaTypeFormatterMatch);
			}
			else if ((mediaTypeFormatterMatch = MatchRequestMediaType(request, mediaTypeFormatter)) != null)
			{
				listWrapperCollection.Add(mediaTypeFormatterMatch);
			}
			else if (ShouldMatchOnType(enumerable) && (mediaTypeFormatterMatch = MatchType(type, mediaTypeFormatter)) != null)
			{
				listWrapperCollection.Add(mediaTypeFormatterMatch);
			}
		}
		return listWrapperCollection;
	}

	protected virtual MediaTypeFormatterMatch SelectResponseMediaTypeFormatter(ICollection<MediaTypeFormatterMatch> matches)
	{
		if (matches == null)
		{
			throw Error.ArgumentNull("matches");
		}
		List<MediaTypeFormatterMatch> list = matches.AsList();
		MediaTypeFormatterMatch mediaTypeFormatterMatch = null;
		MediaTypeFormatterMatch mediaTypeFormatterMatch2 = null;
		MediaTypeFormatterMatch mediaTypeFormatterMatch3 = null;
		MediaTypeFormatterMatch mediaTypeFormatterMatch4 = null;
		MediaTypeFormatterMatch mediaTypeFormatterMatch5 = null;
		MediaTypeFormatterMatch mediaTypeFormatterMatch6 = null;
		for (int i = 0; i < list.Count; i++)
		{
			MediaTypeFormatterMatch mediaTypeFormatterMatch7 = list[i];
			switch (mediaTypeFormatterMatch7.Ranking)
			{
			case MediaTypeFormatterMatchRanking.MatchOnCanWriteType:
				if (mediaTypeFormatterMatch == null)
				{
					mediaTypeFormatterMatch = mediaTypeFormatterMatch7;
				}
				break;
			case MediaTypeFormatterMatchRanking.MatchOnRequestWithMediaTypeMapping:
				mediaTypeFormatterMatch5 = UpdateBestMatch(mediaTypeFormatterMatch5, mediaTypeFormatterMatch7);
				break;
			case MediaTypeFormatterMatchRanking.MatchOnRequestAcceptHeaderLiteral:
				mediaTypeFormatterMatch2 = UpdateBestMatch(mediaTypeFormatterMatch2, mediaTypeFormatterMatch7);
				break;
			case MediaTypeFormatterMatchRanking.MatchOnRequestAcceptHeaderSubtypeMediaRange:
				mediaTypeFormatterMatch3 = UpdateBestMatch(mediaTypeFormatterMatch3, mediaTypeFormatterMatch7);
				break;
			case MediaTypeFormatterMatchRanking.MatchOnRequestAcceptHeaderAllMediaRange:
				mediaTypeFormatterMatch4 = UpdateBestMatch(mediaTypeFormatterMatch4, mediaTypeFormatterMatch7);
				break;
			case MediaTypeFormatterMatchRanking.MatchOnRequestMediaType:
				if (mediaTypeFormatterMatch6 == null)
				{
					mediaTypeFormatterMatch6 = mediaTypeFormatterMatch7;
				}
				break;
			}
		}
		if (mediaTypeFormatterMatch5 != null)
		{
			MediaTypeFormatterMatch current = mediaTypeFormatterMatch5;
			current = UpdateBestMatch(current, mediaTypeFormatterMatch2);
			current = UpdateBestMatch(current, mediaTypeFormatterMatch3);
			current = UpdateBestMatch(current, mediaTypeFormatterMatch4);
			if (current != mediaTypeFormatterMatch5)
			{
				mediaTypeFormatterMatch5 = null;
			}
		}
		MediaTypeFormatterMatch mediaTypeFormatterMatch8 = null;
		if (mediaTypeFormatterMatch5 != null)
		{
			mediaTypeFormatterMatch8 = mediaTypeFormatterMatch5;
		}
		else if (mediaTypeFormatterMatch2 != null || mediaTypeFormatterMatch3 != null || mediaTypeFormatterMatch4 != null)
		{
			mediaTypeFormatterMatch8 = UpdateBestMatch(mediaTypeFormatterMatch8, mediaTypeFormatterMatch2);
			mediaTypeFormatterMatch8 = UpdateBestMatch(mediaTypeFormatterMatch8, mediaTypeFormatterMatch3);
			mediaTypeFormatterMatch8 = UpdateBestMatch(mediaTypeFormatterMatch8, mediaTypeFormatterMatch4);
		}
		else if (mediaTypeFormatterMatch6 != null)
		{
			mediaTypeFormatterMatch8 = mediaTypeFormatterMatch6;
		}
		else if (mediaTypeFormatterMatch != null)
		{
			mediaTypeFormatterMatch8 = mediaTypeFormatterMatch;
		}
		return mediaTypeFormatterMatch8;
	}

	protected virtual Encoding SelectResponseCharacterEncoding(HttpRequestMessage request, MediaTypeFormatter formatter)
	{
		if (request == null)
		{
			throw Error.ArgumentNull("request");
		}
		if (formatter == null)
		{
			throw Error.ArgumentNull("formatter");
		}
		List<Encoding> supportedEncodingsInternal = formatter.SupportedEncodingsInternal;
		if (supportedEncodingsInternal.Count > 0)
		{
			foreach (StringWithQualityHeaderValue item in SortStringWithQualityHeaderValuesByQFactor(request.Headers.AcceptCharset))
			{
				for (int i = 0; i < supportedEncodingsInternal.Count; i++)
				{
					Encoding encoding = supportedEncodingsInternal[i];
					if (encoding != null && item.Quality != 0.0 && (item.Value.Equals(encoding.WebName, StringComparison.OrdinalIgnoreCase) || item.Value.Equals("*", StringComparison.OrdinalIgnoreCase)))
					{
						return encoding;
					}
				}
			}
			return formatter.SelectCharacterEncoding((request.Content != null) ? request.Content.Headers : null);
		}
		return null;
	}

	protected virtual MediaTypeFormatterMatch MatchMediaTypeMapping(HttpRequestMessage request, MediaTypeFormatter formatter)
	{
		if (request == null)
		{
			throw Error.ArgumentNull("request");
		}
		if (formatter == null)
		{
			throw Error.ArgumentNull("formatter");
		}
		List<MediaTypeMapping> mediaTypeMappingsInternal = formatter.MediaTypeMappingsInternal;
		for (int i = 0; i < mediaTypeMappingsInternal.Count; i++)
		{
			MediaTypeMapping mediaTypeMapping = mediaTypeMappingsInternal[i];
			double value;
			if (mediaTypeMapping != null && (value = mediaTypeMapping.TryMatchMediaType(request)) > 0.0)
			{
				return new MediaTypeFormatterMatch(formatter, mediaTypeMapping.MediaType, value, MediaTypeFormatterMatchRanking.MatchOnRequestWithMediaTypeMapping);
			}
		}
		return null;
	}

	protected virtual MediaTypeFormatterMatch MatchAcceptHeader(IEnumerable<MediaTypeWithQualityHeaderValue> sortedAcceptValues, MediaTypeFormatter formatter)
	{
		if (sortedAcceptValues == null)
		{
			throw Error.ArgumentNull("sortedAcceptValues");
		}
		if (formatter == null)
		{
			throw Error.ArgumentNull("formatter");
		}
		foreach (MediaTypeWithQualityHeaderValue sortedAcceptValue in sortedAcceptValues)
		{
			List<MediaTypeHeaderValue> supportedMediaTypesInternal = formatter.SupportedMediaTypesInternal;
			for (int i = 0; i < supportedMediaTypesInternal.Count; i++)
			{
				MediaTypeHeaderValue mediaTypeHeaderValue = supportedMediaTypesInternal[i];
				if (mediaTypeHeaderValue != null && sortedAcceptValue.Quality != 0.0 && mediaTypeHeaderValue.IsSubsetOf(sortedAcceptValue, out var mediaType2Range))
				{
					return new MediaTypeFormatterMatch(ranking: mediaType2Range switch
					{
						MediaTypeHeaderValueRange.AllMediaRange => MediaTypeFormatterMatchRanking.MatchOnRequestAcceptHeaderAllMediaRange, 
						MediaTypeHeaderValueRange.SubtypeMediaRange => MediaTypeFormatterMatchRanking.MatchOnRequestAcceptHeaderSubtypeMediaRange, 
						_ => MediaTypeFormatterMatchRanking.MatchOnRequestAcceptHeaderLiteral, 
					}, formatter: formatter, mediaType: mediaTypeHeaderValue, quality: sortedAcceptValue.Quality);
				}
			}
		}
		return null;
	}

	protected virtual MediaTypeFormatterMatch MatchRequestMediaType(HttpRequestMessage request, MediaTypeFormatter formatter)
	{
		if (request == null)
		{
			throw Error.ArgumentNull("request");
		}
		if (formatter == null)
		{
			throw Error.ArgumentNull("formatter");
		}
		if (request.Content != null)
		{
			MediaTypeHeaderValue contentType = request.Content.Headers.ContentType;
			if (contentType != null)
			{
				List<MediaTypeHeaderValue> supportedMediaTypesInternal = formatter.SupportedMediaTypesInternal;
				for (int i = 0; i < supportedMediaTypesInternal.Count; i++)
				{
					MediaTypeHeaderValue mediaTypeHeaderValue = supportedMediaTypesInternal[i];
					if (mediaTypeHeaderValue != null && mediaTypeHeaderValue.IsSubsetOf(contentType))
					{
						return new MediaTypeFormatterMatch(formatter, mediaTypeHeaderValue, 1.0, MediaTypeFormatterMatchRanking.MatchOnRequestMediaType);
					}
				}
			}
		}
		return null;
	}

	protected virtual bool ShouldMatchOnType(IEnumerable<MediaTypeWithQualityHeaderValue> sortedAcceptValues)
	{
		if (sortedAcceptValues == null)
		{
			throw Error.ArgumentNull("sortedAcceptValues");
		}
		if (ExcludeMatchOnTypeOnly)
		{
			return !sortedAcceptValues.Any();
		}
		return true;
	}

	protected virtual MediaTypeFormatterMatch MatchType(Type type, MediaTypeFormatter formatter)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (formatter == null)
		{
			throw Error.ArgumentNull("formatter");
		}
		MediaTypeHeaderValue mediaType = null;
		List<MediaTypeHeaderValue> supportedMediaTypesInternal = formatter.SupportedMediaTypesInternal;
		if (supportedMediaTypesInternal.Count > 0)
		{
			mediaType = supportedMediaTypesInternal[0];
		}
		return new MediaTypeFormatterMatch(formatter, mediaType, 1.0, MediaTypeFormatterMatchRanking.MatchOnCanWriteType);
	}

	protected virtual IEnumerable<MediaTypeWithQualityHeaderValue> SortMediaTypeWithQualityHeaderValuesByQFactor(ICollection<MediaTypeWithQualityHeaderValue> headerValues)
	{
		if (headerValues == null)
		{
			throw Error.ArgumentNull("headerValues");
		}
		if (headerValues.Count > 1)
		{
			return headerValues.OrderByDescending((MediaTypeWithQualityHeaderValue m) => m, MediaTypeWithQualityHeaderValueComparer.QualityComparer).ToArray();
		}
		return headerValues;
	}

	protected virtual IEnumerable<StringWithQualityHeaderValue> SortStringWithQualityHeaderValuesByQFactor(ICollection<StringWithQualityHeaderValue> headerValues)
	{
		if (headerValues == null)
		{
			throw Error.ArgumentNull("headerValues");
		}
		if (headerValues.Count > 1)
		{
			return headerValues.OrderByDescending((StringWithQualityHeaderValue m) => m, StringWithQualityHeaderValueComparer.QualityComparer).ToArray();
		}
		return headerValues;
	}

	protected virtual MediaTypeFormatterMatch UpdateBestMatch(MediaTypeFormatterMatch current, MediaTypeFormatterMatch potentialReplacement)
	{
		if (potentialReplacement == null)
		{
			return current;
		}
		if (current != null)
		{
			if (!(potentialReplacement.Quality > current.Quality))
			{
				return current;
			}
			return potentialReplacement;
		}
		return potentialReplacement;
	}

	private static MediaTypeFormatter[] GetWritingFormatters(IEnumerable<MediaTypeFormatter> formatters)
	{
		if (formatters is MediaTypeFormatterCollection mediaTypeFormatterCollection)
		{
			return mediaTypeFormatterCollection.WritingFormatters;
		}
		return formatters.AsArray();
	}
}
