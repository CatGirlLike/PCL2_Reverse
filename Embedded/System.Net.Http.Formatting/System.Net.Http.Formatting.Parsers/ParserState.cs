namespace System.Net.Http.Formatting.Parsers;

internal enum ParserState
{
	NeedMoreData,
	Done,
	Invalid,
	DataTooBig
}
