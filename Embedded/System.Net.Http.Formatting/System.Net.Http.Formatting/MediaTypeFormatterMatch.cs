using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http.Formatting;

public class MediaTypeFormatterMatch
{
	public MediaTypeFormatter Formatter { get; private set; }

	public MediaTypeHeaderValue MediaType { get; private set; }

	public double Quality { get; private set; }

	public MediaTypeFormatterMatchRanking Ranking { get; private set; }

	public MediaTypeFormatterMatch(MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType, double? quality, MediaTypeFormatterMatchRanking ranking)
	{
		if (formatter == null)
		{
			throw Error.ArgumentNull("formatter");
		}
		Formatter = formatter;
		MediaType = ((mediaType != null) ? mediaType.Clone() : MediaTypeConstants.ApplicationOctetStreamMediaType);
		Quality = quality ?? 1.0;
		Ranking = ranking;
	}
}
