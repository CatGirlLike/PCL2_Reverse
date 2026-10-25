using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Internal;
using System.Net.Http.Properties;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using System.Xml.Serialization;

namespace System.Net.Http.Formatting;

public class XmlMediaTypeFormatter : MediaTypeFormatter
{
	private ConcurrentDictionary<Type, object> _serializerCache = new ConcurrentDictionary<Type, object>();

	private XmlDictionaryReaderQuotas _readerQuotas = FormattingUtilities.CreateDefaultReaderQuotas();

	public static MediaTypeHeaderValue DefaultMediaType => MediaTypeConstants.ApplicationXmlMediaType;

	[DefaultValue(false)]
	public bool UseXmlSerializer { get; set; }

	public bool Indent
	{
		get
		{
			return WriterSettings.Indent;
		}
		set
		{
			WriterSettings.Indent = value;
		}
	}

	public XmlWriterSettings WriterSettings { get; private set; }

	public int MaxDepth
	{
		get
		{
			return _readerQuotas.MaxDepth;
		}
		set
		{
			if (value < 1)
			{
				throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 1);
			}
			_readerQuotas.MaxDepth = value;
		}
	}

	public XmlMediaTypeFormatter()
	{
		base.SupportedMediaTypes.Add(MediaTypeConstants.ApplicationXmlMediaType);
		base.SupportedMediaTypes.Add(MediaTypeConstants.TextXmlMediaType);
		base.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
		base.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
		WriterSettings = new XmlWriterSettings
		{
			OmitXmlDeclaration = true,
			CloseOutput = false,
			CheckCharacters = false
		};
	}

	protected XmlMediaTypeFormatter(XmlMediaTypeFormatter formatter)
		: base(formatter)
	{
		UseXmlSerializer = formatter.UseXmlSerializer;
		WriterSettings = formatter.WriterSettings;
		MaxDepth = formatter.MaxDepth;
	}

	public void SetSerializer(Type type, XmlObjectSerializer serializer)
	{
		VerifyAndSetSerializer(type, serializer);
	}

	public void SetSerializer<T>(XmlObjectSerializer serializer)
	{
		SetSerializer(typeof(T), serializer);
	}

	public void SetSerializer(Type type, XmlSerializer serializer)
	{
		VerifyAndSetSerializer(type, serializer);
	}

	public void SetSerializer<T>(XmlSerializer serializer)
	{
		SetSerializer(typeof(T), serializer);
	}

	public bool RemoveSerializer(Type type)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		object value;
		return _serializerCache.TryRemove(type, out value);
	}

	public override bool CanReadType(Type type)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		return GetCachedSerializer(type, throwOnError: false) != null;
	}

	public override bool CanWriteType(Type type)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (UseXmlSerializer)
		{
			MediaTypeFormatter.TryGetDelegatingTypeForIEnumerableGenericOrSame(ref type);
		}
		else
		{
			MediaTypeFormatter.TryGetDelegatingTypeForIQueryableGenericOrSame(ref type);
		}
		return GetCachedSerializer(type, throwOnError: false) != null;
	}

	public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (readStream == null)
		{
			throw Error.ArgumentNull("readStream");
		}
		try
		{
			return Task.FromResult(ReadFromStream(type, readStream, content, formatterLogger));
		}
		catch (Exception exception)
		{
			return TaskHelpers.FromError<object>(exception);
		}
	}

	private object ReadFromStream(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
	{
		HttpContentHeaders httpContentHeaders = content?.Headers;
		if (httpContentHeaders != null && httpContentHeaders.ContentLength == 0)
		{
			return MediaTypeFormatter.GetDefaultValueForType(type);
		}
		object deserializer = GetDeserializer(type, content);
		try
		{
			using XmlReader xmlReader = CreateXmlReader(readStream, content);
			if (deserializer is XmlSerializer xmlSerializer)
			{
				return xmlSerializer.Deserialize(xmlReader);
			}
			XmlObjectSerializer obj = deserializer as XmlObjectSerializer;
			if (obj == null)
			{
				ThrowInvalidSerializerException(deserializer, "GetDeserializer");
			}
			return obj.ReadObject(xmlReader);
		}
		catch (Exception exception)
		{
			if (formatterLogger == null)
			{
				throw;
			}
			formatterLogger.LogError(string.Empty, exception);
			return MediaTypeFormatter.GetDefaultValueForType(type);
		}
	}

	protected internal virtual object GetDeserializer(Type type, HttpContent content)
	{
		return GetSerializerForType(type);
	}

	protected internal virtual XmlReader CreateXmlReader(Stream readStream, HttpContent content)
	{
		Encoding encoding = SelectCharacterEncoding(content?.Headers);
		return XmlDictionaryReader.CreateTextReader(new NonClosingDelegatingStream(readStream), encoding, _readerQuotas, null);
	}

	public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext, CancellationToken cancellationToken)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (writeStream == null)
		{
			throw Error.ArgumentNull("writeStream");
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return TaskHelpers.Canceled();
		}
		try
		{
			WriteToStream(type, value, writeStream, content);
			return TaskHelpers.Completed();
		}
		catch (Exception exception)
		{
			return TaskHelpers.FromError(exception);
		}
	}

	private void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
	{
		bool flag = false;
		flag = ((!UseXmlSerializer) ? MediaTypeFormatter.TryGetDelegatingTypeForIQueryableGenericOrSame(ref type) : MediaTypeFormatter.TryGetDelegatingTypeForIEnumerableGenericOrSame(ref type));
		if (flag && value != null)
		{
			value = MediaTypeFormatter.GetTypeRemappingConstructor(type).Invoke(new object[1] { value });
		}
		object serializer = GetSerializer(type, value, content);
		using XmlWriter xmlWriter = CreateXmlWriter(writeStream, content);
		if (serializer is XmlSerializer xmlSerializer)
		{
			xmlSerializer.Serialize(xmlWriter, value);
			return;
		}
		XmlObjectSerializer obj = serializer as XmlObjectSerializer;
		if (obj == null)
		{
			ThrowInvalidSerializerException(serializer, "GetSerializer");
		}
		obj.WriteObject(xmlWriter, value);
	}

	protected internal virtual object GetSerializer(Type type, object value, HttpContent content)
	{
		return GetSerializerForType(type);
	}

	protected internal virtual XmlWriter CreateXmlWriter(Stream writeStream, HttpContent content)
	{
		Encoding encoding = SelectCharacterEncoding(content?.Headers);
		XmlWriterSettings xmlWriterSettings = WriterSettings.Clone();
		xmlWriterSettings.Encoding = encoding;
		return XmlWriter.Create(writeStream, xmlWriterSettings);
	}

	public virtual XmlSerializer CreateXmlSerializer(Type type)
	{
		return new XmlSerializer(type);
	}

	public virtual DataContractSerializer CreateDataContractSerializer(Type type)
	{
		return new DataContractSerializer(type);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public XmlReader InvokeCreateXmlReader(Stream readStream, HttpContent content)
	{
		return CreateXmlReader(readStream, content);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public XmlWriter InvokeCreateXmlWriter(Stream writeStream, HttpContent content)
	{
		return CreateXmlWriter(writeStream, content);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public object InvokeGetDeserializer(Type type, HttpContent content)
	{
		return GetDeserializer(type, content);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public object InvokeGetSerializer(Type type, object value, HttpContent content)
	{
		return GetSerializer(type, value, content);
	}

	private object CreateDefaultSerializer(Type type, bool throwOnError)
	{
		Exception ex = null;
		object obj = null;
		try
		{
			if (UseXmlSerializer)
			{
				obj = CreateXmlSerializer(type);
			}
			else
			{
				FormattingUtilities.XsdDataContractExporter.GetRootElementName(type);
				obj = CreateDataContractSerializer(type);
			}
		}
		catch (Exception ex2)
		{
			ex = ex2;
		}
		if (obj == null && throwOnError)
		{
			if (ex != null)
			{
				throw Error.InvalidOperation(ex, System.Net.Http.Properties.Resources.SerializerCannotSerializeType, UseXmlSerializer ? typeof(XmlSerializer).Name : typeof(DataContractSerializer).Name, type.Name);
			}
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.SerializerCannotSerializeType, UseXmlSerializer ? typeof(XmlSerializer).Name : typeof(DataContractSerializer).Name, type.Name);
		}
		return obj;
	}

	private object GetCachedSerializer(Type type, bool throwOnError)
	{
		if (!_serializerCache.TryGetValue(type, out var value))
		{
			value = CreateDefaultSerializer(type, throwOnError);
			_serializerCache.TryAdd(type, value);
		}
		return value;
	}

	private void VerifyAndSetSerializer(Type type, object serializer)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (serializer == null)
		{
			throw Error.ArgumentNull("serializer");
		}
		SetSerializerInternal(type, serializer);
	}

	private void SetSerializerInternal(Type type, object serializer)
	{
		_serializerCache.AddOrUpdate(type, serializer, (Type key, object value) => serializer);
	}

	private object GetSerializerForType(Type type)
	{
		object cachedSerializer = GetCachedSerializer(type, throwOnError: true);
		if (cachedSerializer == null)
		{
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.SerializerCannotSerializeType, UseXmlSerializer ? typeof(XmlSerializer).Name : typeof(DataContractSerializer).Name, type.Name);
		}
		return cachedSerializer;
	}

	private static void ThrowInvalidSerializerException(object serializer, string getSerializerMethodName)
	{
		if (serializer == null)
		{
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.XmlMediaTypeFormatter_NullReturnedSerializer, getSerializerMethodName);
		}
		throw Error.InvalidOperation(System.Net.Http.Properties.Resources.XmlMediaTypeFormatter_InvalidSerializerType, serializer.GetType().Name, getSerializerMethodName);
	}
}
