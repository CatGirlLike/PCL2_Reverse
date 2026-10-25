using System.Collections.Specialized;
using System.IO;
using System.Net.Http.Formatting.Internal;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http;

public class MultipartFormDataStreamProvider : MultipartFileStreamProvider
{
	private CancellationToken _cancellationToken;

	public NameValueCollection FormData { get; private set; }

	public MultipartFormDataStreamProvider(string rootPath)
		: base(rootPath)
	{
		FormData = HttpValueCollection.Create();
	}

	public MultipartFormDataStreamProvider(string rootPath, int bufferSize)
		: base(rootPath, bufferSize)
	{
		FormData = HttpValueCollection.Create();
	}

	public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
	{
		if (MultipartFormDataStreamProviderHelper.IsFileContent(parent, headers))
		{
			return base.GetStream(parent, headers);
		}
		return new MemoryStream();
	}

	public override Task ExecutePostProcessingAsync()
	{
		return MultipartFormDataStreamProviderHelper.ReadFormDataAsync(base.Contents, FormData, _cancellationToken);
	}

	public override Task ExecutePostProcessingAsync(CancellationToken cancellationToken)
	{
		_cancellationToken = cancellationToken;
		return ExecutePostProcessingAsync();
	}
}
