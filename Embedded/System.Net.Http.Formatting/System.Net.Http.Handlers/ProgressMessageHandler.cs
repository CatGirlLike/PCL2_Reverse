using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http.Handlers;

public class ProgressMessageHandler : DelegatingHandler
{
	public event EventHandler<HttpProgressEventArgs> HttpSendProgress;

	public event EventHandler<HttpProgressEventArgs> HttpReceiveProgress;

	public ProgressMessageHandler()
	{
	}

	public ProgressMessageHandler(HttpMessageHandler innerHandler)
		: base(innerHandler)
	{
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		AddRequestProgress(request);
		HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
		if (this.HttpReceiveProgress != null && response != null && response.Content != null)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await AddResponseProgressAsync(request, response);
		}
		return response;
	}

	protected internal virtual void OnHttpRequestProgress(HttpRequestMessage request, HttpProgressEventArgs e)
	{
		if (this.HttpSendProgress != null)
		{
			this.HttpSendProgress(request, e);
		}
	}

	protected internal virtual void OnHttpResponseProgress(HttpRequestMessage request, HttpProgressEventArgs e)
	{
		if (this.HttpReceiveProgress != null)
		{
			this.HttpReceiveProgress(request, e);
		}
	}

	private void AddRequestProgress(HttpRequestMessage request)
	{
		if (this.HttpSendProgress != null && request != null && request.Content != null)
		{
			HttpContent content = new ProgressContent(request.Content, this, request);
			request.Content = content;
		}
	}

	private async Task<HttpResponseMessage> AddResponseProgressAsync(HttpRequestMessage request, HttpResponseMessage response)
	{
		HttpContent httpContent = new StreamContent(new ProgressStream(await response.Content.ReadAsStreamAsync(), this, request, response));
		response.Content.Headers.CopyTo(httpContent.Headers);
		response.Content = httpContent;
		return response;
	}
}
