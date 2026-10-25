using System.Collections.Concurrent;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Internal;
using System.Net.Http.Properties;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using Newtonsoft.Json;

namespace System.Net.Http.Formatting;

public class JsonMediaTypeFormatter : BaseJsonMediaTypeFormatter
{
	private ConcurrentDictionary<Type, DataContractJsonSerializer> _dataContractSerializerCache = new ConcurrentDictionary<Type, DataContractJsonSerializer>();

	private XmlDictionaryReaderQuotas _readerQuotas = FormattingUtilities.CreateDefaultReaderQuotas();

	private RequestHeaderMapping _requestHeaderMapping;

	public static MediaTypeHeaderValue DefaultMediaType => MediaTypeConstants.ApplicationJsonMediaType;

	public bool UseDataContractJsonSerializer { get; set; }

	public bool Indent { get; set; }

	public sealed override int MaxDepth
	{
		get
		{
			return base.MaxDepth;
		}
		set
		{
			base.MaxDepth = value;
			_readerQuotas.MaxDepth = value;
		}
	}

	public JsonMediaTypeFormatter()
	{
		base.SupportedMediaTypes.Add(MediaTypeConstants.ApplicationJsonMediaType);
		base.SupportedMediaTypes.Add(MediaTypeConstants.TextJsonMediaType);
		_requestHeaderMapping = new XmlHttpRequestHeaderMapping();
		base.MediaTypeMappings.Add(_requestHeaderMapping);
	}

	protected JsonMediaTypeFormatter(JsonMediaTypeFormatter formatter)
		: base(formatter)
	{
		UseDataContractJsonSerializer = formatter.UseDataContractJsonSerializer;
		Indent = formatter.Indent;
	}

	public override JsonReader CreateJsonReader(Type type, Stream readStream, Encoding effectiveEncoding)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (readStream == null)
		{
			throw Error.ArgumentNull("readStream");
		}
		if (effectiveEncoding == null)
		{
			throw Error.ArgumentNull("effectiveEncoding");
		}
		return new JsonTextReader(new StreamReader(readStream, effectiveEncoding));
	}

	public override JsonWriter CreateJsonWriter(Type type, Stream writeStream, Encoding effectiveEncoding)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (writeStream == null)
		{
			throw Error.ArgumentNull("writeStream");
		}
		if (effectiveEncoding == null)
		{
			throw Error.ArgumentNull("effectiveEncoding");
		}
		JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(writeStream, effectiveEncoding));
		if (Indent)
		{
			jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
		}
		return jsonWriter;
	}

	public override bool CanReadType(Type type)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (UseDataContractJsonSerializer)
		{
			return _dataContractSerializerCache.GetOrAdd(type, (Type t) => CreateDataContractSerializer(t, throwOnError: false)) != null;
		}
		return base.CanReadType(type);
	}

	public override bool CanWriteType(Type type)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (UseDataContractJsonSerializer)
		{
			MediaTypeFormatter.TryGetDelegatingTypeForIQueryableGenericOrSame(ref type);
			return _dataContractSerializerCache.GetOrAdd(type, (Type t) => CreateDataContractSerializer(t, throwOnError: false)) != null;
		}
		return base.CanWriteType(type);
	}

	public override object ReadFromStream(Type type, Stream readStream, Encoding effectiveEncoding, IFormatterLogger formatterLogger)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (readStream == null)
		{
			throw Error.ArgumentNull("readStream");
		}
		if (effectiveEncoding == null)
		{
			throw Error.ArgumentNull("effectiveEncoding");
		}
		if (UseDataContractJsonSerializer)
		{
			DataContractJsonSerializer dataContractSerializer = GetDataContractSerializer(type);
			using XmlReader reader = JsonReaderWriterFactory.CreateJsonReader(new NonClosingDelegatingStream(readStream), effectiveEncoding, _readerQuotas, null);
			return dataContractSerializer.ReadObject(reader);
		}
		return base.ReadFromStream(type, readStream, effectiveEncoding, formatterLogger);
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
		if (UseDataContractJsonSerializer && Indent)
		{
			throw Error.NotSupported(System.Net.Http.Properties.Resources.UnsupportedIndent, typeof(DataContractJsonSerializer));
		}
		return base.WriteToStreamAsync(type, value, writeStream, content, transportContext, cancellationToken);
	}

	public override void WriteToStream(Type type, object value, Stream writeStream, Encoding effectiveEncoding)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (writeStream == null)
		{
			throw Error.ArgumentNull("writeStream");
		}
		if (effectiveEncoding == null)
		{
			throw Error.ArgumentNull("effectiveEncoding");
		}
		if (UseDataContractJsonSerializer)
		{
			if (MediaTypeFormatter.TryGetDelegatingTypeForIQueryableGenericOrSame(ref type) && value != null)
			{
				value = MediaTypeFormatter.GetTypeRemappingConstructor(type).Invoke(new object[1] { value });
			}
			DataContractJsonSerializer dataContractSerializer = GetDataContractSerializer(type);
			using XmlWriter writer = JsonReaderWriterFactory.CreateJsonWriter(writeStream, effectiveEncoding, ownsStream: false);
			dataContractSerializer.WriteObject(writer, value);
			return;
		}
		base.WriteToStream(type, value, writeStream, effectiveEncoding);
	}

	private DataContractJsonSerializer CreateDataContractSerializer(Type type, bool throwOnError)
	{
		DataContractJsonSerializer dataContractJsonSerializer = null;
		Exception ex = null;
		try
		{
			FormattingUtilities.XsdDataContractExporter.GetRootElementName(type);
			dataContractJsonSerializer = CreateDataContractSerializer(type);
		}
		catch (Exception ex2)
		{
			ex = ex2;
		}
		if (dataContractJsonSerializer == null && throwOnError)
		{
			if (ex != null)
			{
				throw Error.InvalidOperation(ex, System.Net.Http.Properties.Resources.SerializerCannotSerializeType, typeof(DataContractJsonSerializer).Name, type.Name);
			}
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.SerializerCannotSerializeType, typeof(DataContractJsonSerializer).Name, type.Name);
		}
		return dataContractJsonSerializer;
	}

	public virtual DataContractJsonSerializer CreateDataContractSerializer(Type type)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		return new DataContractJsonSerializer(type);
	}

	private DataContractJsonSerializer GetDataContractSerializer(Type type)
	{
		DataContractJsonSerializer orAdd = _dataContractSerializerCache.GetOrAdd(type, (Type t) => CreateDataContractSerializer(type, throwOnError: true));
		if (orAdd == null)
		{
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.SerializerCannotSerializeType, typeof(DataContractJsonSerializer).Name, type.Name);
		}
		return orAdd;
	}
}
