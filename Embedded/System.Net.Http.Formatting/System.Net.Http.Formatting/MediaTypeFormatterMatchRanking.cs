namespace System.Net.Http.Formatting;

public enum MediaTypeFormatterMatchRanking
{
	None,
	MatchOnCanWriteType,
	MatchOnRequestAcceptHeaderLiteral,
	MatchOnRequestAcceptHeaderSubtypeMediaRange,
	MatchOnRequestAcceptHeaderAllMediaRange,
	MatchOnRequestWithMediaTypeMapping,
	MatchOnRequestMediaType
}
