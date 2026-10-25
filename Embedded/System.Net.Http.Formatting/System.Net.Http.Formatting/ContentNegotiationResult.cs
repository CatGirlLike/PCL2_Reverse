using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http.Formatting;

public class ContentNegotiationResult
{
	private MediaTypeFormatter _formatter;

	public MediaTypeFormatter Formatter
	{
		get
		{
			return _formatter;
		}
		set
		{
			if (value == null)
			{
				throw Error.ArgumentNull("value");
			}
			_formatter = value;
		}
	}

	public MediaTypeHeaderValue MediaType { get; set; }

	public ContentNegotiationResult(MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType)
	{
		if (formatter == null)
		{
			throw Error.ArgumentNull("formatter");
		}
		_formatter = formatter;
		MediaType = mediaType;
	}
}
