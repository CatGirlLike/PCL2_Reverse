using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;

namespace System.Net.Http.Formatting;

public class MediaTypeFormatterCollection : Collection<MediaTypeFormatter>
{
	private static readonly Type _mediaTypeFormatterType = typeof(MediaTypeFormatter);

	private MediaTypeFormatter[] _writingFormatters;

	public XmlMediaTypeFormatter XmlFormatter => base.Items.OfType<XmlMediaTypeFormatter>().FirstOrDefault();

	public JsonMediaTypeFormatter JsonFormatter => base.Items.OfType<JsonMediaTypeFormatter>().FirstOrDefault();

	public FormUrlEncodedMediaTypeFormatter FormUrlEncodedFormatter => base.Items.OfType<FormUrlEncodedMediaTypeFormatter>().FirstOrDefault();

	internal MediaTypeFormatter[] WritingFormatters
	{
		get
		{
			if (_writingFormatters == null)
			{
				_writingFormatters = GetWritingFormatters();
			}
			return _writingFormatters;
		}
	}

	internal event EventHandler Changing;

	public MediaTypeFormatterCollection()
		: this(CreateDefaultFormatters())
	{
	}

	public MediaTypeFormatterCollection(IEnumerable<MediaTypeFormatter> formatters)
	{
		VerifyAndSetFormatters(formatters);
	}

	public void AddRange(IEnumerable<MediaTypeFormatter> items)
	{
		if (items == null)
		{
			throw Error.ArgumentNull("items");
		}
		foreach (MediaTypeFormatter item in items)
		{
			Add(item);
		}
	}

	public void InsertRange(int index, IEnumerable<MediaTypeFormatter> items)
	{
		if (items == null)
		{
			throw Error.ArgumentNull("items");
		}
		foreach (MediaTypeFormatter item in items)
		{
			Insert(index++, item);
		}
	}

	public MediaTypeFormatter FindReader(Type type, MediaTypeHeaderValue mediaType)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (mediaType == null)
		{
			throw Error.ArgumentNull("mediaType");
		}
		foreach (MediaTypeFormatter item in base.Items)
		{
			if (item == null || !item.CanReadType(type))
			{
				continue;
			}
			foreach (MediaTypeHeaderValue supportedMediaType in item.SupportedMediaTypes)
			{
				if (supportedMediaType != null && supportedMediaType.IsSubsetOf(mediaType))
				{
					return item;
				}
			}
		}
		return null;
	}

	public MediaTypeFormatter FindWriter(Type type, MediaTypeHeaderValue mediaType)
	{
		if (type == null)
		{
			throw Error.ArgumentNull("type");
		}
		if (mediaType == null)
		{
			throw Error.ArgumentNull("mediaType");
		}
		foreach (MediaTypeFormatter item in base.Items)
		{
			if (item == null || !item.CanWriteType(type))
			{
				continue;
			}
			foreach (MediaTypeHeaderValue supportedMediaType in item.SupportedMediaTypes)
			{
				if (supportedMediaType != null && supportedMediaType.IsSubsetOf(mediaType))
				{
					return item;
				}
			}
		}
		return null;
	}

	public static bool IsTypeExcludedFromValidation(Type type)
	{
		if (!typeof(XmlNode).IsAssignableFrom(type) && !typeof(FormDataCollection).IsAssignableFrom(type) && !FormattingUtilities.IsJTokenType(type) && !typeof(XObject).IsAssignableFrom(type) && !typeof(Type).IsAssignableFrom(type))
		{
			return type == typeof(byte[]);
		}
		return true;
	}

	protected override void ClearItems()
	{
		OnChanging();
		base.ClearItems();
	}

	protected override void InsertItem(int index, MediaTypeFormatter item)
	{
		OnChanging();
		base.InsertItem(index, item);
	}

	protected override void RemoveItem(int index)
	{
		OnChanging();
		base.RemoveItem(index);
	}

	protected override void SetItem(int index, MediaTypeFormatter item)
	{
		OnChanging();
		base.SetItem(index, item);
	}

	private void OnChanging()
	{
		if (this.Changing != null)
		{
			this.Changing(this, EventArgs.Empty);
		}
		_writingFormatters = null;
	}

	private MediaTypeFormatter[] GetWritingFormatters()
	{
		return base.Items.Where((MediaTypeFormatter formatter) => formatter?.CanWriteAnyTypes ?? false).ToArray();
	}

	private static IEnumerable<MediaTypeFormatter> CreateDefaultFormatters()
	{
		return new MediaTypeFormatter[3]
		{
			new JsonMediaTypeFormatter(),
			new XmlMediaTypeFormatter(),
			new FormUrlEncodedMediaTypeFormatter()
		};
	}

	private void VerifyAndSetFormatters(IEnumerable<MediaTypeFormatter> formatters)
	{
		if (formatters == null)
		{
			throw Error.ArgumentNull("formatters");
		}
		foreach (MediaTypeFormatter formatter in formatters)
		{
			if (formatter == null)
			{
				throw Error.Argument("formatters", System.Net.Http.Properties.Resources.CannotHaveNullInList, _mediaTypeFormatterType.Name);
			}
			Add(formatter);
		}
	}
}
