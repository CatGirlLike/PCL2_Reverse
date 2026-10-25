using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting.Parsers;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http.Formatting;

public class FormUrlEncodedMediaTypeFormatter : MediaTypeFormatter
{
	private const int MinBufferSize = 256;

	private const int DefaultBufferSize = 32768;

	private int _readBufferSize = 32768;

	private int _maxDepth = 256;

	private readonly bool _isDerived;

	public static MediaTypeHeaderValue DefaultMediaType => MediaTypeConstants.ApplicationFormUrlEncodedMediaType;

	public int MaxDepth
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

	public int ReadBufferSize
	{
		get
		{
			return _readBufferSize;
		}
		set
		{
			if (value < 256)
			{
				throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 256);
			}
			_readBufferSize = value;
		}
	}

	internal override bool CanWriteAnyTypes => _isDerived;

	public FormUrlEncodedMediaTypeFormatter()
	{
		base.SupportedMediaTypes.Add(MediaTypeConstants.ApplicationFormUrlEncodedMediaType);
		_isDerived = GetType() != typeof(FormUrlEncodedMediaTypeFormatter);
	}

	protected FormUrlEncodedMediaTypeFormatter(FormUrlEncodedMediaTypeFormatter formatter)
		: base(formatter)
	{
		MaxDepth = formatter.MaxDepth;
		ReadBufferSize = formatter.ReadBufferSize;
		_isDerived = GetType() != typeof(FormUrlEncodedMediaTypeFormatter);
	}

	public override bool CanReadType(Type type)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (!(type == typeof(FormDataCollection)))
		{
			return FormattingUtilities.IsJTokenType(type);
		}
		return true;
	}

	public override bool CanWriteType(Type type)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		return false;
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
			return Task.FromResult(ReadFromStream(type, readStream));
		}
		catch (Exception exception)
		{
			return TaskHelpers.FromError<object>(exception);
		}
	}

	private object ReadFromStream(Type type, Stream readStream)
	{
		IEnumerable<KeyValuePair<string, string>> enumerable = ReadFormUrlEncoded(readStream, ReadBufferSize);
		if (type == typeof(FormDataCollection))
		{
			return new FormDataCollection(enumerable);
		}
		if (FormattingUtilities.IsJTokenType(type))
		{
			return FormUrlEncodedJson.Parse(enumerable, _maxDepth);
		}
		throw Error.InvalidOperation(System.Net.Http.Properties.Resources.SerializerCannotSerializeType, GetType().Name, type.Name);
	}

	private static IEnumerable<KeyValuePair<string, string>> ReadFormUrlEncoded(Stream input, int bufferSize)
	{
		byte[] array = new byte[bufferSize];
		bool flag = false;
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		FormUrlEncodedParser formUrlEncodedParser = new FormUrlEncodedParser(list, long.MaxValue);
		do
		{
			int num;
			try
			{
				num = input.Read(array, 0, array.Length);
				if (num == 0)
				{
					flag = true;
				}
			}
			catch (Exception innerException)
			{
				throw Error.InvalidOperation(innerException, System.Net.Http.Properties.Resources.ErrorReadingFormUrlEncodedStream);
			}
			int bytesConsumed = 0;
			ParserState parserState = formUrlEncodedParser.ParseBuffer(array, num, ref bytesConsumed, flag);
			if (parserState != 0 && parserState != ParserState.Done)
			{
				throw Error.InvalidOperation(System.Net.Http.Properties.Resources.FormUrlEncodedParseError, bytesConsumed);
			}
		}
		while (!flag);
		return list;
	}
}
