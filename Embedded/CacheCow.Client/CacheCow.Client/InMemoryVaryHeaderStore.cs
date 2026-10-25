using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace CacheCow.Client;

public class InMemoryVaryHeaderStore : IVaryHeaderStore, IDisposable
{
	private const string CacheName = "###_IVaryHeaderStore_###";

	private readonly ConcurrentDictionary<string, string[]> _varyHeaderCache = new ConcurrentDictionary<string, string[]>();

	private MemoryCache _cache = new MemoryCache("###_IVaryHeaderStore_###");

	public bool TryGetValue(string uri, out IEnumerable<string> headers)
	{
		headers = (string[])_cache.Get(uri);
		return headers != null;
	}

	public void AddOrUpdate(string uri, IEnumerable<string> headers)
	{
		_cache.Set(uri, headers, DateTimeOffset.MaxValue);
	}

	public bool TryRemove(string uri)
	{
		return _cache.Remove(uri) != null;
	}

	public void Clear()
	{
		((IDisposable)_cache).Dispose();
		_cache = new MemoryCache("###_IVaryHeaderStore_###");
	}

	public void Dispose()
	{
		_cache.Dispose();
	}
}
