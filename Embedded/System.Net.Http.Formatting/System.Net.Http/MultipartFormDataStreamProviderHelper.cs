using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http;

internal static class MultipartFormDataStreamProviderHelper
{
	public static bool IsFileContent(HttpContent parent, HttpContentHeaders headers)
	{
		if (parent == null)
		{
			throw Error.ArgumentNull("parent");
		}
		if (headers == null)
		{
			throw Error.ArgumentNull("headers");
		}
		ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;
		if (contentDisposition == null)
		{
			throw Error.InvalidOperation(System.Net.Http.Properties.Resources.MultipartFormDataStreamProviderNoContentDisposition, "Content-Disposition");
		}
		if (!string.IsNullOrEmpty(contentDisposition.FileName))
		{
			return true;
		}
		return false;
	}

	public static async Task ReadFormDataAsync(Collection<HttpContent> contents, NameValueCollection formData, CancellationToken cancellationToken)
	{
		foreach (HttpContent content in contents)
		{
			ContentDispositionHeaderValue contentDisposition = content.Headers.ContentDisposition;
			if (string.IsNullOrEmpty(contentDisposition.FileName))
			{
				string formFieldName = FormattingUtilities.UnquoteToken(contentDisposition.Name) ?? string.Empty;
				cancellationToken.ThrowIfCancellationRequested();
				formData.Add(formFieldName, await content.ReadAsStringAsync());
			}
		}
	}
}
