using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http.Formatting;

public abstract class MediaTypeFormatter
{
	internal class MediaTypeHeaderValueCollection : Collection<MediaTypeHeaderValue>
	{
		private static readonly Type _mediaTypeHeaderValueType = typeof(MediaTypeHeaderValue);

		internal MediaTypeHeaderValueCollection(IList<MediaTypeHeaderValue> list)
			: base(list)
		{
		}

		protected override void InsertItem(int index, MediaTypeHeaderValue item)
		{
			ValidateMediaType(item);
			base.InsertItem(index, item);
		}

		protected override void SetItem(int index, MediaTypeHeaderValue item)
		{
			ValidateMediaType(item);
			base.SetItem(index, item);
		}

		private static void ValidateMediaType(MediaTypeHeaderValue item)
		{
			if (item == null)
			{
				throw Error.ArgumentNull("item");
			}
			ParsedMediaTypeHeaderValue parsedMediaTypeHeaderValue = new ParsedMediaTypeHeaderValue(item);
			if (parsedMediaTypeHeaderValue.IsAllMediaRange || parsedMediaTypeHeaderValue.IsSubtypeMediaRange)
			{
				throw Error.Argument("item", System.Net.Http.Properties.Resources.CannotUseMediaRangeForSupportedMediaType, _mediaTypeHeaderValueType.Name, item.MediaType);
			}
		}
	}

	private const int DefaultMinHttpCollectionKeys = 1;

	private const int DefaultMaxHttpCollectionKeys = 1000;

	private const string IWellKnownComparerTypeName = "System.IWellKnownStringEqualityComparer, mscorlib, Version=4.0.0.0, PublicKeyToken=b77a5c561934e089";

	private static readonly ConcurrentDictionary<Type, Type> _delegatingEnumerableCache = new ConcurrentDictionary<Type, Type>();

	private static ConcurrentDictionary<Type, ConstructorInfo> _delegatingEnumerableConstructorCache = new ConcurrentDictionary<Type, ConstructorInfo>();

	private static Lazy<int> _defaultMaxHttpCollectionKeys = new Lazy<int>(InitializeDefaultCollectionKeySize, isThreadSafe: true);

	private static int _maxHttpCollectionKeys = -1;

	private readonly List<MediaTypeHeaderValue> _supportedMediaTypes;

	private readonly List<Encoding> _supportedEncodings;

	private readonly List<MediaTypeMapping> _mediaTypeMappings;

	private IRequiredMemberSelector _requiredMemberSelector;

	public static int MaxHttpCollectionKeys
	{
		get
		{
			if (_maxHttpCollectionKeys < 0)
			{
				_maxHttpCollectionKeys = _defaultMaxHttpCollectionKeys.Value;
			}
			return _maxHttpCollectionKeys;
		}
		set
		{
			if (value < 1)
			{
				throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 1);
			}
			_maxHttpCollectionKeys = value;
		}
	}

	public Collection<MediaTypeHeaderValue> SupportedMediaTypes { get; private set; }

	internal List<MediaTypeHeaderValue> SupportedMediaTypesInternal => _supportedMediaTypes;

	public Collection<Encoding> SupportedEncodings { get; private set; }

	internal List<Encoding> SupportedEncodingsInternal => _supportedEncodings;

	public Collection<MediaTypeMapping> MediaTypeMappings { get; private set; }

	internal List<MediaTypeMapping> MediaTypeMappingsInternal => _mediaTypeMappings;

	public virtual IRequiredMemberSelector RequiredMemberSelector
	{
		get
		{
			return _requiredMemberSelector;
		}
		set
		{
			_requiredMemberSelector = value;
		}
	}

	internal virtual bool CanWriteAnyTypes => true;

	protected MediaTypeFormatter()
	{
		_supportedMediaTypes = new List<MediaTypeHeaderValue>();
		SupportedMediaTypes = new MediaTypeHeaderValueCollection(_supportedMediaTypes);
		_supportedEncodings = new List<Encoding>();
		SupportedEncodings = new Collection<Encoding>(_supportedEncodings);
		_mediaTypeMappings = new List<MediaTypeMapping>();
		MediaTypeMappings = new Collection<MediaTypeMapping>(_mediaTypeMappings);
	}

	protected MediaTypeFormatter(MediaTypeFormatter formatter)
	{
		if (formatter == null)
		{
			throw Error.ArgumentNull("formatter");
		}
		_supportedMediaTypes = formatter._supportedMediaTypes;
		SupportedMediaTypes = formatter.SupportedMediaTypes;
		_supportedEncodings = formatter._supportedEncodings;
		SupportedEncodings = formatter.SupportedEncodings;
		_mediaTypeMappings = formatter._mediaTypeMappings;
		MediaTypeMappings = formatter.MediaTypeMappings;
		_requiredMemberSelector = formatter._requiredMemberSelector;
	}

	public virtual Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
	{
		throw Error.NotSupported(System.Net.Http.Properties.Resources.MediaTypeFormatterCannotRead, GetType().Name);
	}

	public virtual Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return TaskHelpers.Canceled<object>();
		}
		return ReadFromStreamAsync(type, readStream, content, formatterLogger);
	}

	public virtual Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
	{
		return WriteToStreamAsync(type, value, writeStream, content, transportContext, CancellationToken.None);
	}

	public virtual Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext, CancellationToken cancellationToken)
	{
		throw Error.NotSupported(System.Net.Http.Properties.Resources.MediaTypeFormatterCannotWrite, GetType().Name);
	}

	private static bool TryGetDelegatingType(Type interfaceType, ref Type type)
	{
		if (type != null && type.IsInterface() && type.IsGenericType())
		{
			Type type2 = type.ExtractGenericInterface(interfaceType);
			if (type2 != null)
			{
				type = GetOrAddDelegatingType(type, type2);
				return true;
			}
		}
		return false;
	}

	private static int InitializeDefaultCollectionKeySize()
	{
		return int.MaxValue;
	}

	internal static bool TryGetDelegatingTypeForIEnumerableGenericOrSame(ref Type type)
	{
		return TryGetDelegatingType(FormattingUtilities.EnumerableInterfaceGenericType, ref type);
	}

	internal static bool TryGetDelegatingTypeForIQueryableGenericOrSame(ref Type type)
	{
		return TryGetDelegatingType(FormattingUtilities.QueryableInterfaceGenericType, ref type);
	}

	internal static ConstructorInfo GetTypeRemappingConstructor(Type type)
	{
		_delegatingEnumerableConstructorCache.TryGetValue(type, out var value);
		return value;
	}

	public Encoding SelectCharacterEncoding(HttpContentHeaders contentHeaders)
	{
		Encoding encoding = null;
		if (contentHeaders != null && contentHeaders.ContentType != null)
		{
			string charSet = contentHeaders.ContentType.CharSet;
			if (!string.IsNullOrWhiteSpace(charSet))
			{
				for (int i = 0; i < _supportedEncodings.Count; i++)
				{
					Encoding encoding2 = _supportedEncodings[i];
					if (charSet.Equals(encoding2.WebName, StringComparison.OrdinalIgnoreCase))
					{
						encoding = encoding2;
						break;
					}
				}
			}
		}
		if (encoding == null && _supportedEncodings.Count > 0)
		{
			encoding = _supportedEncodings[0];
		}
		if (encoding == null)
		{
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.MediaTypeFormatterNoEncoding, GetType().Name);
		}
		return encoding;
	}

	public virtual void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (headers == null)
		{
			throw Error.ArgumentNull("headers");
		}
		if (mediaType != null)
		{
			headers.ContentType = mediaType.Clone();
		}
		if (headers.ContentType == null)
		{
			MediaTypeHeaderValue mediaTypeHeaderValue = null;
			if (_supportedMediaTypes.Count > 0)
			{
				mediaTypeHeaderValue = _supportedMediaTypes[0];
			}
			if (mediaTypeHeaderValue != null)
			{
				headers.ContentType = mediaTypeHeaderValue.Clone();
			}
		}
		if (headers.ContentType != null && headers.ContentType.CharSet == null)
		{
			Encoding encoding = null;
			if (_supportedEncodings.Count > 0)
			{
				encoding = _supportedEncodings[0];
			}
			if (encoding != null)
			{
				headers.ContentType.CharSet = encoding.WebName;
			}
		}
	}

	public virtual MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (request == null)
		{
			throw Error.ArgumentNull("request");
		}
		return this;
	}

	public abstract bool CanReadType(Type type);

	public abstract bool CanWriteType(Type type);

	private static Type GetOrAddDelegatingType(Type type, Type genericType)
	{
		return _delegatingEnumerableCache.GetOrAdd(type, delegate
		{
			Type type2 = genericType.GetGenericArguments()[0];
			Type type3 = FormattingUtilities.DelegatingEnumerableGenericType.MakeGenericType(type2);
			ConstructorInfo constructor = type3.GetConstructor(new Type[1] { FormattingUtilities.EnumerableInterfaceGenericType.MakeGenericType(type2) });
			_delegatingEnumerableConstructorCache.TryAdd(type3, constructor);
			return type3;
		});
	}

	public static object GetDefaultValueForType(Type type)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (type.IsValueType())
		{
			return Activator.CreateInstance(type);
		}
		return null;
	}
}
