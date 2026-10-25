using System.ComponentModel;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class HttpClientExtensions
{
	public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
	{
		return client.PostAsJsonAsync(requestUri, value, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient client, string requestUri, T value, CancellationToken cancellationToken)
	{
		return client.PostAsync(requestUri, value, new JsonMediaTypeFormatter(), cancellationToken);
	}

	public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient client, Uri requestUri, T value)
	{
		return client.PostAsJsonAsync(requestUri, value, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient client, Uri requestUri, T value, CancellationToken cancellationToken)
	{
		return client.PostAsync(requestUri, value, new JsonMediaTypeFormatter(), cancellationToken);
	}

	public static Task<HttpResponseMessage> PostAsXmlAsync<T>(this HttpClient client, string requestUri, T value)
	{
		return client.PostAsXmlAsync(requestUri, value, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PostAsXmlAsync<T>(this HttpClient client, string requestUri, T value, CancellationToken cancellationToken)
	{
		return client.PostAsync(requestUri, value, new XmlMediaTypeFormatter(), cancellationToken);
	}

	public static Task<HttpResponseMessage> PostAsXmlAsync<T>(this HttpClient client, Uri requestUri, T value)
	{
		return client.PostAsXmlAsync(requestUri, value, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PostAsXmlAsync<T>(this HttpClient client, Uri requestUri, T value, CancellationToken cancellationToken)
	{
		return client.PostAsync(requestUri, value, new XmlMediaTypeFormatter(), cancellationToken);
	}

	public static Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter)
	{
		return client.PostAsync(requestUri, value, formatter, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, CancellationToken cancellationToken)
	{
		return client.PostAsync(requestUri, value, formatter, (MediaTypeHeaderValue)null, cancellationToken);
	}

	public static Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, string mediaType)
	{
		return client.PostAsync(requestUri, value, formatter, mediaType, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, string mediaType, CancellationToken cancellationToken)
	{
		return client.PostAsync(requestUri, value, formatter, ObjectContent.BuildHeaderValue(mediaType), cancellationToken);
	}

	public static Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType, CancellationToken cancellationToken)
	{
		if (client == null)
		{
			throw Error.ArgumentNull("client");
		}
		ObjectContent<T> content = new ObjectContent<T>(value, formatter, mediaType);
		return client.PostAsync(requestUri, content, cancellationToken);
	}

	public static Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter)
	{
		return client.PostAsync(requestUri, value, formatter, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter, CancellationToken cancellationToken)
	{
		return client.PostAsync(requestUri, value, formatter, (MediaTypeHeaderValue)null, cancellationToken);
	}

	public static Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter, string mediaType)
	{
		return client.PostAsync(requestUri, value, formatter, mediaType, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter, string mediaType, CancellationToken cancellationToken)
	{
		return client.PostAsync(requestUri, value, formatter, ObjectContent.BuildHeaderValue(mediaType), cancellationToken);
	}

	public static Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType, CancellationToken cancellationToken)
	{
		if (client == null)
		{
			throw Error.ArgumentNull("client");
		}
		ObjectContent<T> content = new ObjectContent<T>(value, formatter, mediaType);
		return client.PostAsync(requestUri, content, cancellationToken);
	}

	public static Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
	{
		return client.PutAsJsonAsync(requestUri, value, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient client, string requestUri, T value, CancellationToken cancellationToken)
	{
		return client.PutAsync(requestUri, value, new JsonMediaTypeFormatter(), cancellationToken);
	}

	public static Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient client, Uri requestUri, T value)
	{
		return client.PutAsJsonAsync(requestUri, value, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient client, Uri requestUri, T value, CancellationToken cancellationToken)
	{
		return client.PutAsync(requestUri, value, new JsonMediaTypeFormatter(), cancellationToken);
	}

	public static Task<HttpResponseMessage> PutAsXmlAsync<T>(this HttpClient client, string requestUri, T value)
	{
		return client.PutAsXmlAsync(requestUri, value, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PutAsXmlAsync<T>(this HttpClient client, string requestUri, T value, CancellationToken cancellationToken)
	{
		return client.PutAsync(requestUri, value, new XmlMediaTypeFormatter(), cancellationToken);
	}

	public static Task<HttpResponseMessage> PutAsXmlAsync<T>(this HttpClient client, Uri requestUri, T value)
	{
		return client.PutAsXmlAsync(requestUri, value, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PutAsXmlAsync<T>(this HttpClient client, Uri requestUri, T value, CancellationToken cancellationToken)
	{
		return client.PutAsync(requestUri, value, new XmlMediaTypeFormatter(), cancellationToken);
	}

	public static Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter)
	{
		return client.PutAsync(requestUri, value, formatter, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, CancellationToken cancellationToken)
	{
		return client.PutAsync(requestUri, value, formatter, (MediaTypeHeaderValue)null, cancellationToken);
	}

	public static Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, string mediaType)
	{
		return client.PutAsync(requestUri, value, formatter, mediaType, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, string mediaType, CancellationToken cancellationToken)
	{
		return client.PutAsync(requestUri, value, formatter, ObjectContent.BuildHeaderValue(mediaType), cancellationToken);
	}

	public static Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType, CancellationToken cancellationToken)
	{
		if (client == null)
		{
			throw Error.ArgumentNull("client");
		}
		ObjectContent<T> content = new ObjectContent<T>(value, formatter, mediaType);
		return client.PutAsync(requestUri, content, cancellationToken);
	}

	public static Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter)
	{
		return client.PutAsync(requestUri, value, formatter, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter, CancellationToken cancellationToken)
	{
		return client.PutAsync(requestUri, value, formatter, (MediaTypeHeaderValue)null, cancellationToken);
	}

	public static Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter, string mediaType)
	{
		return client.PutAsync(requestUri, value, formatter, mediaType, CancellationToken.None);
	}

	public static Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter, string mediaType, CancellationToken cancellationToken)
	{
		return client.PutAsync(requestUri, value, formatter, ObjectContent.BuildHeaderValue(mediaType), cancellationToken);
	}

	public static Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType, CancellationToken cancellationToken)
	{
		if (client == null)
		{
			throw Error.ArgumentNull("client");
		}
		ObjectContent<T> content = new ObjectContent<T>(value, formatter, mediaType);
		return client.PutAsync(requestUri, content, cancellationToken);
	}
}
