using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CacheCow.Client;

public class MessageContentHttpMessageSerializer : IHttpMessageSerializerAsync
{
	private bool _bufferContent;

	public MessageContentHttpMessageSerializer()
		: this(bufferContent: true)
	{
	}

	public MessageContentHttpMessageSerializer(bool bufferContent)
	{
		_bufferContent = bufferContent;
	}

	public async Task SerializeAsync(HttpResponseMessage response, Stream stream)
	{
		if (response.Content != null)
		{
			TraceWriter.WriteLine("SerializeAsync - before load", TraceLevel.Verbose);
			_ = response.Content.Headers.ContentLength;
			if (_bufferContent)
			{
				await response.Content.LoadIntoBufferAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			TraceWriter.WriteLine("SerializeAsync - after load", TraceLevel.Verbose);
		}
		else
		{
			TraceWriter.WriteLine("Content NULL - before load", TraceLevel.Verbose);
		}
		byte[] array = await new HttpMessageContent(response).ReadAsByteArrayAsync();
		TraceWriter.WriteLine("SerializeAsync - after ReadAsByteArrayAsync", TraceLevel.Verbose);
		stream.Write(array, 0, array.Length);
	}

	public async Task SerializeAsync(HttpRequestMessage request, Stream stream)
	{
		if (request.Content != null && _bufferContent)
		{
			await request.Content.LoadIntoBufferAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		byte[] array = await new HttpMessageContent(request).ReadAsByteArrayAsync().ConfigureAwait(continueOnCapturedContext: false);
		stream.Write(array, 0, array.Length);
	}

	public async Task<HttpResponseMessage> DeserializeToResponseAsync(Stream stream)
	{
		HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
		httpResponseMessage.Content = new StreamContent(stream);
		httpResponseMessage.Content.Headers.Add("Content-Type", "application/http;msgtype=response");
		TraceWriter.WriteLine("before ReadAsHttpResponseMessageAsync", TraceLevel.Verbose);
		HttpResponseMessage responseMessage = await httpResponseMessage.Content.ReadAsHttpResponseMessageAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (responseMessage.Content != null && _bufferContent)
		{
			await responseMessage.Content.LoadIntoBufferAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		if (responseMessage.Content == null)
		{
			TraceWriter.WriteLine("Content is NULL desering from cache", TraceLevel.Warning);
		}
		return responseMessage;
	}

	public async Task<HttpRequestMessage> DeserializeToRequestAsync(Stream stream)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
		httpRequestMessage.Content = new StreamContent(stream);
		httpRequestMessage.Content.Headers.Add("Content-Type", "application/http;msgtype=request");
		HttpRequestMessage requestMessage = await httpRequestMessage.Content.ReadAsHttpRequestMessageAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (requestMessage.Content != null && _bufferContent)
		{
			await requestMessage.Content.LoadIntoBufferAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		return requestMessage;
	}
}
