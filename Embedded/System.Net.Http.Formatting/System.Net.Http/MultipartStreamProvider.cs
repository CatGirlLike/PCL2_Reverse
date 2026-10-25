using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http;

public abstract class MultipartStreamProvider
{
	private Collection<HttpContent> _contents = new Collection<HttpContent>();

	public Collection<HttpContent> Contents => _contents;

	public abstract Stream GetStream(HttpContent parent, HttpContentHeaders headers);

	public virtual Task ExecutePostProcessingAsync()
	{
		return TaskHelpers.Completed();
	}

	public virtual Task ExecutePostProcessingAsync(CancellationToken cancellationToken)
	{
		return ExecutePostProcessingAsync();
	}
}
