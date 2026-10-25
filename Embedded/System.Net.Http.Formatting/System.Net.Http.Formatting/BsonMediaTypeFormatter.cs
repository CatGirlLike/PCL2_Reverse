using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace System.Net.Http.Formatting;

public class BsonMediaTypeFormatter : BaseJsonMediaTypeFormatter
{
	private static readonly Type OpenDictionaryType = typeof(Dictionary<, >);

	public static MediaTypeHeaderValue DefaultMediaType => MediaTypeConstants.ApplicationBsonMediaType;

	public sealed override int MaxDepth
	{
		get
		{
			return base.MaxDepth;
		}
		set
		{
			base.MaxDepth = value;
		}
	}

	public BsonMediaTypeFormatter()
	{
		base.SupportedMediaTypes.Add(MediaTypeConstants.ApplicationBsonMediaType);
	}

	protected BsonMediaTypeFormatter(BsonMediaTypeFormatter formatter)
		: base(formatter)
	{
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
		if (type == typeof(DBNull) && content != null && content.Headers != null && content.Headers.ContentLength == 0)
		{
			return Task.FromResult((object)DBNull.Value);
		}
		return base.ReadFromStreamAsync(type, readStream, content, formatterLogger);
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
		if (IsSimpleType(type) || type == typeof(byte[]))
		{
			Type type2 = OpenDictionaryType.MakeGenericType(typeof(string), type);
			if (!(base.ReadFromStream(type2, readStream, effectiveEncoding, formatterLogger) is IDictionary dictionary))
			{
				throw Error.InvalidOperation(System.Net.Http.Properties.Resources.MediaTypeFormatter_BsonParseError_MissingData, type2.Name);
			}
			string text = string.Empty;
			{
				IDictionaryEnumerator dictionaryEnumerator = dictionary.GetEnumerator();
				try
				{
					if (dictionaryEnumerator.MoveNext())
					{
						DictionaryEntry dictionaryEntry = (DictionaryEntry)dictionaryEnumerator.Current;
						if (dictionary.Count == 1 && dictionaryEntry.Key as string == "Value")
						{
							return dictionaryEntry.Value;
						}
						if (dictionaryEntry.Key != null)
						{
							text = dictionaryEntry.Key.ToString();
						}
					}
				}
				finally
				{
					IDisposable disposable = dictionaryEnumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.MediaTypeFormatter_BsonParseError_UnexpectedData, dictionary.Count, text);
		}
		return base.ReadFromStream(type, readStream, effectiveEncoding, formatterLogger);
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
		BsonReader bsonReader = new BsonReader(new BinaryReader(readStream, effectiveEncoding));
		try
		{
			bsonReader.ReadRootValueAsArray = typeof(IEnumerable).IsAssignableFrom(type) && !typeof(IDictionary).IsAssignableFrom(type);
			return bsonReader;
		}
		catch
		{
			((IDisposable)bsonReader).Dispose();
			throw;
		}
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
		if (value != null && value != DBNull.Value)
		{
			Type type2 = value.GetType();
			if (IsSimpleType(type2) || type2 == typeof(byte[]))
			{
				Dictionary<string, object> value2 = new Dictionary<string, object> { { "Value", value } };
				base.WriteToStream(typeof(Dictionary<string, object>), value2, writeStream, effectiveEncoding);
			}
			else
			{
				base.WriteToStream(type, value, writeStream, effectiveEncoding);
			}
		}
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
		return new BsonWriter(new BinaryWriter(writeStream, effectiveEncoding));
	}

	private static bool IsSimpleType(Type type)
	{
		return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
	}
}
