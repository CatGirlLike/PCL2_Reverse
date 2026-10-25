using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http.Formatting.Internal;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http;

public abstract class MultipartFormDataRemoteStreamProvider : MultipartStreamProvider
{
	private CancellationToken _cancellationToken = CancellationToken.None;

	public Collection<MultipartRemoteFileData> FileData { get; private set; }

	public NameValueCollection FormData { get; private set; }

	protected MultipartFormDataRemoteStreamProvider()
	{
		FormData = HttpValueCollection.Create();
		FileData = new Collection<MultipartRemoteFileData>();
	}

	public abstract RemoteStreamInfo GetRemoteStream(HttpContent parent, HttpContentHeaders headers);

	public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
	{
		if (MultipartFormDataStreamProviderHelper.IsFileContent(parent, headers))
		{
			RemoteStreamInfo remoteStream = GetRemoteStream(parent, headers);
			if (remoteStream == null)
			{
				throw Error.InvalidOperation(System.Net.Http.Properties.Resources.RemoteStreamInfoCannotBeNull, "GetRemoteStream", GetType().Name);
			}
			FileData.Add(new MultipartRemoteFileData(headers, remoteStream.Location, remoteStream.FileName));
			return remoteStream.RemoteStream;
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
