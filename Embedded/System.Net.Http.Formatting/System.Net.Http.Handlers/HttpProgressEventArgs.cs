using System.ComponentModel;

namespace System.Net.Http.Handlers;

public class HttpProgressEventArgs : ProgressChangedEventArgs
{
	public long BytesTransferred { get; private set; }

	public long? TotalBytes { get; private set; }

	public HttpProgressEventArgs(int progressPercentage, object userToken, long bytesTransferred, long? totalBytes)
		: base(progressPercentage, userToken)
	{
		BytesTransferred = bytesTransferred;
		TotalBytes = totalBytes;
	}
}
