using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http;

public class ObjectContent<T> : ObjectContent
{
	public ObjectContent(T value, MediaTypeFormatter formatter)
		: this(value, formatter, (MediaTypeHeaderValue)null)
	{
	}

	public ObjectContent(T value, MediaTypeFormatter formatter, string mediaType)
		: this(value, formatter, ObjectContent.BuildHeaderValue(mediaType))
	{
	}

	public ObjectContent(T value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType)
		: base(typeof(T), value, formatter, mediaType)
	{
	}
}
public class ObjectContent : HttpContent
{
	private object _value;

	private readonly MediaTypeFormatter _formatter;

	public Type ObjectType { get; private set; }

	public MediaTypeFormatter Formatter => _formatter;

	public object Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	public ObjectContent(Type type, object value, MediaTypeFormatter formatter)
		: this(type, value, formatter, (MediaTypeHeaderValue)null)
	{
	}

	public ObjectContent(Type type, object value, MediaTypeFormatter formatter, string mediaType)
		: this(type, value, formatter, BuildHeaderValue(mediaType))
	{
	}

	public ObjectContent(Type type, object value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (formatter == null)
		{
			throw Error.ArgumentNull("formatter");
		}
		if (!formatter.CanWriteType(type))
		{
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.ObjectContent_FormatterCannotWriteType, formatter.GetType().FullName, type.Name);
		}
		_formatter = formatter;
		ObjectType = type;
		VerifyAndSetObject(value);
		_formatter.SetDefaultContentHeaders(type, base.Headers, mediaType);
	}

	internal static MediaTypeHeaderValue BuildHeaderValue(string mediaType)
	{
		if (mediaType == null)
		{
			return null;
		}
		return new MediaTypeHeaderValue(mediaType);
	}

	protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
	{
		return _formatter.WriteToStreamAsync(ObjectType, Value, stream, this, context);
	}

	protected override bool TryComputeLength(out long length)
	{
		length = -1L;
		return false;
	}

	private static bool IsTypeNullable(Type type)
	{
		if (type.IsValueType())
		{
			if (type.IsGenericType())
			{
				return type.GetGenericTypeDefinition() == typeof(Nullable<>);
			}
			return false;
		}
		return true;
	}

	private void VerifyAndSetObject(object value)
	{
		if (value == null)
		{
			if (!IsTypeNullable(ObjectType))
			{
				throw Error.InvalidOperation(System.Net.Http.Properties.Resources.CannotUseNullValueType, typeof(ObjectContent).Name, ObjectType.Name);
			}
		}
		else
		{
			Type type = value.GetType();
			if (!ObjectType.IsAssignableFrom(type))
			{
				throw Error.Argument("value", System.Net.Http.Properties.Resources.ObjectAndTypeDisagree, type.Name, ObjectType.Name);
			}
		}
		_value = value;
	}
}
