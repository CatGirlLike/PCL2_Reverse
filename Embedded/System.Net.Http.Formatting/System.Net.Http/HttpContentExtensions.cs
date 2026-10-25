using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class HttpContentExtensions
{
	private static MediaTypeFormatterCollection _defaultMediaTypeFormatterCollection;

	private static MediaTypeFormatterCollection DefaultMediaTypeFormatterCollection
	{
		get
		{
			if (_defaultMediaTypeFormatterCollection == null)
			{
				_defaultMediaTypeFormatterCollection = new MediaTypeFormatterCollection();
			}
			return _defaultMediaTypeFormatterCollection;
		}
	}

	public static Task<object> ReadAsAsync(this HttpContent content, Type type)
	{
		return content.ReadAsAsync(type, DefaultMediaTypeFormatterCollection);
	}

	public static Task<object> ReadAsAsync(this HttpContent content, Type type, CancellationToken cancellationToken)
	{
		return content.ReadAsAsync(type, DefaultMediaTypeFormatterCollection, cancellationToken);
	}

	public static Task<object> ReadAsAsync(this HttpContent content, Type type, IEnumerable<MediaTypeFormatter> formatters)
	{
		return ReadAsAsync<object>(content, type, formatters, null);
	}

	public static Task<object> ReadAsAsync(this HttpContent content, Type type, IEnumerable<MediaTypeFormatter> formatters, CancellationToken cancellationToken)
	{
		return ReadAsAsync<object>(content, type, formatters, null, cancellationToken);
	}

	public static Task<object> ReadAsAsync(this HttpContent content, Type type, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger)
	{
		return ReadAsAsync<object>(content, type, formatters, formatterLogger);
	}

	public static Task<object> ReadAsAsync(this HttpContent content, Type type, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
	{
		return ReadAsAsync<object>(content, type, formatters, formatterLogger, cancellationToken);
	}

	public static Task<T> ReadAsAsync<T>(this HttpContent content)
	{
		return content.ReadAsAsync<T>(DefaultMediaTypeFormatterCollection);
	}

	public static Task<T> ReadAsAsync<T>(this HttpContent content, CancellationToken cancellationToken)
	{
		return content.ReadAsAsync<T>(DefaultMediaTypeFormatterCollection, cancellationToken);
	}

	public static Task<T> ReadAsAsync<T>(this HttpContent content, IEnumerable<MediaTypeFormatter> formatters)
	{
		return ReadAsAsync<T>(content, typeof(T), formatters, null);
	}

	public static Task<T> ReadAsAsync<T>(this HttpContent content, IEnumerable<MediaTypeFormatter> formatters, CancellationToken cancellationToken)
	{
		return ReadAsAsync<T>(content, typeof(T), formatters, null, cancellationToken);
	}

	public static Task<T> ReadAsAsync<T>(this HttpContent content, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger)
	{
		return ReadAsAsync<T>(content, typeof(T), formatters, formatterLogger);
	}

	public static Task<T> ReadAsAsync<T>(this HttpContent content, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
	{
		return ReadAsAsync<T>(content, typeof(T), formatters, formatterLogger, cancellationToken);
	}

	private static Task<T> ReadAsAsync<T>(HttpContent content, Type type, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger)
	{
		return ReadAsAsync<T>(content, type, formatters, formatterLogger, CancellationToken.None);
	}

	private static Task<T> ReadAsAsync<T>(HttpContent content, Type type, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
	{
		if (content == null)
		{
			throw Error.ArgumentNull("content");
		}
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (formatters == null)
		{
			throw Error.ArgumentNull("formatters");
		}
		if (content is ObjectContent { Value: not null } objectContent && type.IsAssignableFrom(objectContent.Value.GetType()))
		{
			return Task.FromResult((T)objectContent.Value);
		}
		MediaTypeFormatter mediaTypeFormatter = null;
		MediaTypeHeaderValue mediaTypeHeaderValue = content.Headers.ContentType ?? MediaTypeConstants.ApplicationOctetStreamMediaType;
		mediaTypeFormatter = new MediaTypeFormatterCollection(formatters).FindReader(type, mediaTypeHeaderValue);
		if (mediaTypeFormatter == null)
		{
			if (content.Headers.ContentLength == 0)
			{
				return Task.FromResult((T)MediaTypeFormatter.GetDefaultValueForType(type));
			}
			throw new UnsupportedMediaTypeException(Error.Format(System.Net.Http.Properties.Resources.NoReadSerializerAvailable, type.Name, mediaTypeHeaderValue.MediaType), mediaTypeHeaderValue);
		}
		return ReadAsAsyncCore<T>(content, type, formatterLogger, mediaTypeFormatter, cancellationToken);
	}

	private static async Task<T> ReadAsAsyncCore<T>(HttpContent content, Type type, IFormatterLogger formatterLogger, MediaTypeFormatter formatter, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return (T)(await formatter.ReadFromStreamAsync(type, await content.ReadAsStreamAsync(), content, formatterLogger, cancellationToken));
	}
}
