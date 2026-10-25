using System.IO;
using System.Web.Http;

namespace System.Net.Http;

public class RemoteStreamInfo
{
	public string FileName { get; private set; }

	public string Location { get; private set; }

	public Stream RemoteStream { get; private set; }

	public RemoteStreamInfo(Stream remoteStream, string location, string fileName)
	{
		if (remoteStream == null)
		{
			throw Error.ArgumentNull("remoteStream");
		}
		if (location == null)
		{
			throw Error.ArgumentNull("location");
		}
		if (fileName == null)
		{
			throw Error.ArgumentNull("fileName");
		}
		FileName = fileName;
		RemoteStream = remoteStream;
		Location = location;
	}
}
