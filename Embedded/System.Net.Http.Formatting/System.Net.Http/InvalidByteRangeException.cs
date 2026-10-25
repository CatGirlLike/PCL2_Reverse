using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web.Http;

namespace System.Net.Http;

public class InvalidByteRangeException : Exception
{
	public ContentRangeHeaderValue ContentRange { get; private set; }

	public InvalidByteRangeException(ContentRangeHeaderValue contentRange)
	{
		Initialize(contentRange);
	}

	public InvalidByteRangeException(ContentRangeHeaderValue contentRange, string message)
		: base(message)
	{
		Initialize(contentRange);
	}

	public InvalidByteRangeException(ContentRangeHeaderValue contentRange, string message, Exception innerException)
		: base(message, innerException)
	{
		Initialize(contentRange);
	}

	public InvalidByteRangeException(ContentRangeHeaderValue contentRange, SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		Initialize(contentRange);
	}

	private void Initialize(ContentRangeHeaderValue contentRange)
	{
		if (contentRange == null)
		{
			throw Error.ArgumentNull("contentRange");
		}
		ContentRange = contentRange;
	}
}
