using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CacheCow.Common;

namespace CacheCow.Client.FileCacheStore;

public class FileStore : ICacheStore, IDisposable
{
	private readonly MessageContentHttpMessageSerializer _serializer = new MessageContentHttpMessageSerializer();

	private readonly string _cacheRoot;

	private static readonly List<string> ForbiddenDirectories = new List<string> { "/", "", ".", ".." };

	public TimeSpan MinExpiry { get; set; }

	public FileStore(string cacheRoot)
	{
		if (cacheRoot == null || ForbiddenDirectories.Contains(cacheRoot))
		{
			throw new ArgumentException("The given caching directory is null or invalid. Do give an explicit caching directory, not empty, '/' or '.'. This will prevent accidents when cleaning the cache");
		}
		_cacheRoot = cacheRoot;
		if (!Directory.Exists(_cacheRoot))
		{
			Directory.CreateDirectory(cacheRoot);
		}
	}

	public async Task<HttpResponseMessage> GetValueAsync(CacheKey key)
	{
		if (!File.Exists(_pathFor(key)))
		{
			return null;
		}
		using FileStream fs = File.OpenRead(_pathFor(key));
		return await _serializer.DeserializeToResponseAsync(fs);
	}

	public async Task AddOrUpdateAsync(CacheKey key, HttpResponseMessage response)
	{
		using FileStream fs = File.Open(_pathFor(key), FileMode.Create, FileAccess.Write, FileShare.None);
		await _serializer.SerializeAsync(response, fs);
	}

	public async Task<bool> TryRemoveAsync(CacheKey key)
	{
		if (!File.Exists(_pathFor(key)))
		{
			return false;
		}
		File.Delete(_pathFor(key));
		return true;
	}

	public async Task ClearAsync()
	{
		string[] files = Directory.GetFiles(_cacheRoot);
		for (int i = 0; i < files.Length; i++)
		{
			File.Delete(files[i]);
		}
	}

	private string _pathFor(CacheKey key)
	{
		return _cacheRoot + "/" + key.HashBase64.Replace('/', '!');
	}

	public void Dispose()
	{
	}

	public bool IsEmpty()
	{
		return Directory.GetFiles(_cacheRoot).Length == 0;
	}
}
