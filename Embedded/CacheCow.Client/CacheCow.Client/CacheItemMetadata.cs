using System;

namespace CacheCow.Client;

public class CacheItemMetadata
{
	public byte[] Key { get; set; }

	public DateTime LastAccessed { get; set; }

	public long Size { get; set; }

	public string Domain { get; set; }
}
