using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;

namespace System.Net.Http;

public class MultipartRelatedStreamProvider : MultipartStreamProvider
{
	private const string RelatedSubType = "related";

	private const string ContentID = "Content-ID";

	private const string StartParameter = "Start";

	private HttpContent _rootContent;

	private HttpContent _parent;

	public HttpContent RootContent
	{
		get
		{
			if (_rootContent == null)
			{
				_rootContent = FindRootContent(_parent, base.Contents);
			}
			return _rootContent;
		}
	}

	public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
	{
		if (parent == null)
		{
			throw Error.ArgumentNull("parent");
		}
		if (headers == null)
		{
			throw Error.ArgumentNull("headers");
		}
		if (_parent == null)
		{
			_parent = parent;
		}
		return new MemoryStream();
	}

	private static HttpContent FindRootContent(HttpContent parent, IEnumerable<HttpContent> children)
	{
		NameValueHeaderValue nameValueHeaderValue = FindMultipartRelatedParameter(parent, "Start");
		if (nameValueHeaderValue == null)
		{
			return children.FirstOrDefault();
		}
		string startValue = FormattingUtilities.UnquoteToken(nameValueHeaderValue.Value);
		IEnumerable<string> values;
		return children.FirstOrDefault((HttpContent content) => content.Headers.TryGetValues("Content-ID", out values) && string.Equals(FormattingUtilities.UnquoteToken(values.ElementAt(0)), startValue, StringComparison.OrdinalIgnoreCase));
	}

	private static NameValueHeaderValue FindMultipartRelatedParameter(HttpContent content, string parameterName)
	{
		if (content == null)
		{
			return null;
		}
		MediaTypeHeaderValue contentType = content.Headers.ContentType;
		if (contentType == null || !content.IsMimeMultipartContent("related"))
		{
			return null;
		}
		return contentType.Parameters.FirstOrDefault((NameValueHeaderValue nvp) => string.Equals(nvp.Name, parameterName, StringComparison.OrdinalIgnoreCase));
	}
}
