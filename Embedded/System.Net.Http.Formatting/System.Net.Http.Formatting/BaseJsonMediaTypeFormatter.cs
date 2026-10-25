using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace System.Net.Http.Formatting;

public abstract class BaseJsonMediaTypeFormatter : MediaTypeFormatter
{
	private int _maxDepth = 256;

	private readonly IContractResolver _defaultContractResolver;

	private JsonSerializerSettings _jsonSerializerSettings;

	public JsonSerializerSettings SerializerSettings
	{
		get
		{
			return _jsonSerializerSettings;
		}
		set
		{
			if (value == null)
			{
				throw Error.ArgumentNull("value");
			}
			_jsonSerializerSettings = value;
		}
	}

	public virtual int MaxDepth
	{
		get
		{
			return _maxDepth;
		}
		set
		{
			if (value < 1)
			{
				throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 1);
			}
			_maxDepth = value;
		}
	}

	protected BaseJsonMediaTypeFormatter()
	{
		_defaultContractResolver = new JsonContractResolver(this);
		_jsonSerializerSettings = CreateDefaultSerializerSettings();
		base.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
		base.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
	}

	protected BaseJsonMediaTypeFormatter(BaseJsonMediaTypeFormatter formatter)
		: base(formatter)
	{
		SerializerSettings = formatter.SerializerSettings;
		MaxDepth = formatter._maxDepth;
	}

	public JsonSerializerSettings CreateDefaultSerializerSettings()
	{
		return new JsonSerializerSettings
		{
			ContractResolver = _defaultContractResolver,
			MissingMemberHandling = MissingMemberHandling.Ignore,
			TypeNameHandling = TypeNameHandling.None
		};
	}

	public override bool CanReadType(Type type)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		return true;
	}

	public override bool CanWriteType(Type type)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		return true;
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
		Encoding effectiveEncoding = SelectCharacterEncoding(httpContentHeaders);
		try
		{
			return ReadFromStream(type, readStream, effectiveEncoding, formatterLogger);
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

	public virtual object ReadFromStream(Type type, Stream readStream, Encoding effectiveEncoding, IFormatterLogger formatterLogger)
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
		using JsonReader jsonReader = CreateJsonReaderInternal(type, readStream, effectiveEncoding);
		jsonReader.CloseInput = false;
		jsonReader.MaxDepth = _maxDepth;
		JsonSerializer jsonSerializer = CreateJsonSerializerInternal();
		EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> eventHandler = null;
		if (formatterLogger != null)
		{
			eventHandler = delegate(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
			{
				Exception error = e.ErrorContext.Error;
				formatterLogger.LogError(e.ErrorContext.Path, error);
				e.ErrorContext.Handled = true;
			};
			jsonSerializer.Error += eventHandler;
		}
		try
		{
			return jsonSerializer.Deserialize(jsonReader, type);
		}
		finally
		{
			if (eventHandler != null)
			{
				jsonSerializer.Error -= eventHandler;
			}
		}
	}

	private JsonReader CreateJsonReaderInternal(Type type, Stream readStream, Encoding effectiveEncoding)
	{
		JsonReader jsonReader = CreateJsonReader(type, readStream, effectiveEncoding);
		if (jsonReader == null)
		{
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.MediaTypeFormatter_JsonReaderFactoryReturnedNull, "CreateJsonReader");
		}
		return jsonReader;
	}

	public abstract JsonReader CreateJsonReader(Type type, Stream readStream, Encoding effectiveEncoding);

	private JsonSerializer CreateJsonSerializerInternal()
	{
		JsonSerializer jsonSerializer = null;
		try
		{
			jsonSerializer = CreateJsonSerializer();
		}
		catch (Exception innerException)
		{
			throw Error.InvalidOperation(innerException, System.Net.Http.Properties.Resources.JsonSerializerFactoryThrew, "CreateJsonSerializer");
		}
		if (jsonSerializer == null)
		{
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.JsonSerializerFactoryReturnedNull, "CreateJsonSerializer");
		}
		return jsonSerializer;
	}

	public virtual JsonSerializer CreateJsonSerializer()
	{
		return JsonSerializer.Create(SerializerSettings);
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
		Encoding effectiveEncoding = SelectCharacterEncoding(content?.Headers);
		WriteToStream(type, value, writeStream, effectiveEncoding);
	}

	public virtual void WriteToStream(Type type, object value, Stream writeStream, Encoding effectiveEncoding)
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
		using JsonWriter jsonWriter = CreateJsonWriterInternal(type, writeStream, effectiveEncoding);
		jsonWriter.CloseOutput = false;
		CreateJsonSerializerInternal().Serialize(jsonWriter, value);
		jsonWriter.Flush();
	}

	private JsonWriter CreateJsonWriterInternal(Type type, Stream writeStream, Encoding effectiveEncoding)
	{
		JsonWriter jsonWriter = CreateJsonWriter(type, writeStream, effectiveEncoding);
		if (jsonWriter == null)
		{
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.MediaTypeFormatter_JsonWriterFactoryReturnedNull, "CreateJsonWriter");
		}
		return jsonWriter;
	}

	public abstract JsonWriter CreateJsonWriter(Type type, Stream writeStream, Encoding effectiveEncoding);
}
