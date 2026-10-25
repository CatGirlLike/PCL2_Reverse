using System.IO;

namespace System.Net.Http.Internal;

internal class NonClosingDelegatingStream : DelegatingStream
{
	public NonClosingDelegatingStream(Stream innerStream)
		: base(innerStream)
	{
	}

	public override void Close()
	{
	}
}
