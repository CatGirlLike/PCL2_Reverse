using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class HttpContentFormDataExtensions
{
	private const string ApplicationFormUrlEncoded = "application/x-www-form-urlencoded";

	public static bool IsFormData(this HttpContent content)
	{
		if (content == null)
		{
			throw Error.ArgumentNull("content");
		}
		MediaTypeHeaderValue contentType = content.Headers.ContentType;
		if (contentType != null)
		{
			return string.Equals("application/x-www-form-urlencoded", contentType.MediaType, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public static Task<NameValueCollection> ReadAsFormDataAsync(this HttpContent content)
	{
		return content.ReadAsFormDataAsync(CancellationToken.None);
	}

	public static Task<NameValueCollection> ReadAsFormDataAsync(this HttpContent content, CancellationToken cancellationToken)
	{
		if (content == null)
		{
			throw Error.ArgumentNull("content");
		}
		MediaTypeFormatter[] formatters = new MediaTypeFormatter[1]
		{
			new FormUrlEncodedMediaTypeFormatter()
		};
		return ReadAsAsyncCore(content, formatters, cancellationToken);
	}

	private static async Task<NameValueCollection> ReadAsAsyncCore(HttpContent content, MediaTypeFormatter[] formatters, CancellationToken cancellationToken)
	{
		return (await content.ReadAsAsync<FormDataCollection>(formatters, cancellationToken))?.ReadAsNameValueCollection();
	}
}
