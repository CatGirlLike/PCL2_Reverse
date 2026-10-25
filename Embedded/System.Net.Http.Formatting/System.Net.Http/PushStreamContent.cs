using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Http.Internal;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http;

public class PushStreamContent : HttpContent
{
	internal class CompleteTaskOnCloseStream : DelegatingStream
	{
		private TaskCompletionSource<bool> _serializeToStreamTask;

		public CompleteTaskOnCloseStream(Stream innerStream, TaskCompletionSource<bool> serializeToStreamTask)
			: base(innerStream)
		{
			_serializeToStreamTask = serializeToStreamTask;
		}

		public override void Close()
		{
			_serializeToStreamTask.TrySetResult(result: true);
		}
	}

	private readonly Func<Stream, HttpContent, TransportContext, Task> _onStreamAvailable;

	public PushStreamContent(Action<Stream, HttpContent, TransportContext> onStreamAvailable)
		: this(Taskify(onStreamAvailable), (MediaTypeHeaderValue)null)
	{
	}

	public PushStreamContent(Func<Stream, HttpContent, TransportContext, Task> onStreamAvailable)
		: this(onStreamAvailable, (MediaTypeHeaderValue)null)
	{
	}

	public PushStreamContent(Action<Stream, HttpContent, TransportContext> onStreamAvailable, string mediaType)
		: this(Taskify(onStreamAvailable), new MediaTypeHeaderValue(mediaType))
	{
	}

	public PushStreamContent(Func<Stream, HttpContent, TransportContext, Task> onStreamAvailable, string mediaType)
		: this(onStreamAvailable, new MediaTypeHeaderValue(mediaType))
	{
	}

	public PushStreamContent(Action<Stream, HttpContent, TransportContext> onStreamAvailable, MediaTypeHeaderValue mediaType)
		: this(Taskify(onStreamAvailable), mediaType)
	{
	}

	public PushStreamContent(Func<Stream, HttpContent, TransportContext, Task> onStreamAvailable, MediaTypeHeaderValue mediaType)
	{
		if (onStreamAvailable == null)
		{
			throw Error.ArgumentNull("onStreamAvailable");
		}
		_onStreamAvailable = onStreamAvailable;
		base.Headers.ContentType = mediaType ?? MediaTypeConstants.ApplicationOctetStreamMediaType;
	}

	private static Func<Stream, HttpContent, TransportContext, Task> Taskify(Action<Stream, HttpContent, TransportContext> onStreamAvailable)
	{
		if (onStreamAvailable == null)
		{
			throw Error.ArgumentNull("onStreamAvailable");
		}
		return delegate(Stream stream, HttpContent content, TransportContext transportContext)
		{
			onStreamAvailable(stream, content, transportContext);
			return TaskHelpers.Completed();
		};
	}

	protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
	{
		TaskCompletionSource<bool> serializeToStreamTask = new TaskCompletionSource<bool>();
		Stream arg = new CompleteTaskOnCloseStream(stream, serializeToStreamTask);
		await _onStreamAvailable(arg, this, context);
		await serializeToStreamTask.Task;
	}

	protected override bool TryComputeLength(out long length)
	{
		length = -1L;
		return false;
	}
}
