using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Properties;
using System.Web.Http;

namespace System.Net.Http;

public class MultipartFileStreamProvider : MultipartStreamProvider
{
	private const int MinBufferSize = 1;

	private const int DefaultBufferSize = 4096;

	private string _rootPath;

	private int _bufferSize = 4096;

	private Collection<MultipartFileData> _fileData = new Collection<MultipartFileData>();

	public Collection<MultipartFileData> FileData => _fileData;

	protected string RootPath => _rootPath;

	protected int BufferSize => _bufferSize;

	public MultipartFileStreamProvider(string rootPath)
		: this(rootPath, 4096)
	{
	}

	public MultipartFileStreamProvider(string rootPath, int bufferSize)
	{
		if (rootPath == null)
		{
			throw Error.ArgumentNull("rootPath");
		}
		if (bufferSize < 1)
		{
			throw Error.ArgumentMustBeGreaterThanOrEqualTo("bufferSize", bufferSize, 1);
		}
		_rootPath = Path.GetFullPath(rootPath);
		_bufferSize = bufferSize;
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
		string text;
		try
		{
			string localFileName = GetLocalFileName(headers);
			text = Path.Combine(_rootPath, Path.GetFileName(localFileName));
		}
		catch (Exception innerException)
		{
			throw Error.InvalidOperation(innerException, System.Net.Http.Properties.Resources.MultipartStreamProviderInvalidLocalFileName);
		}
		MultipartFileData item = new MultipartFileData(headers, text);
		_fileData.Add(item);
		return File.Create(text, _bufferSize, FileOptions.Asynchronous);
	}

	public virtual string GetLocalFileName(HttpContentHeaders headers)
	{
		if (headers == null)
		{
			throw Error.ArgumentNull("headers");
		}
		return string.Format(CultureInfo.InvariantCulture, "BodyPart_{0}", new object[1] { Guid.NewGuid() });
	}
}
