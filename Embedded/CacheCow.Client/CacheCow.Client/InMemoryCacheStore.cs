using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using CacheCow.Common;
using CacheCow.Common.Helpers;

namespace CacheCow.Client;

public class InMemoryCacheStore : ICacheStore, IDisposable
{
	private const string CacheStoreEntryName = "###InMemoryCacheStore_###";

	private static TimeSpan MinCacheExpiry = TimeSpan.FromHours(6.0);

	private MemoryCache _responseCache;

	private MessageContentHttpMessageSerializer _messageSerializer = new MessageContentHttpMessageSerializer(bufferContent: true);

	private readonly TimeSpan _minExpiry;

	public InMemoryCacheStore()
		: this(MinCacheExpiry)
	{
	}

	public InMemoryCacheStore(TimeSpan minExpiry)
		: this(minExpiry, new MemoryCache("###InMemoryCacheStore_###"))
	{
	}

	private InMemoryCacheStore(TimeSpan minExpiry, MemoryCache cache)
	{
		_minExpiry = minExpiry;
		_responseCache = cache;
	}

	public void Dispose()
	{
		_responseCache.Dispose();
	}

	public async Task<HttpResponseMessage> GetValueAsync(CacheKey key)
	{
		byte[] array = (byte[])_responseCache.Get(key.HashBase64);
		if (array == null)
		{
			return null;
		}
		return await _messageSerializer.DeserializeToResponseAsync(new MemoryStream(array)).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task AddOrUpdateAsync(CacheKey key, HttpResponseMessage response)
	{
		HttpRequestMessage req = response.RequestMessage;
		response.RequestMessage = null;
		MemoryStream memoryStream = new MemoryStream();
		await _messageSerializer.SerializeAsync(response, memoryStream).ConfigureAwait(continueOnCapturedContext: false);
		byte[] value = memoryStream.ToArray();
		response.RequestMessage = req;
		DateTimeOffset dateTimeOffset = response.GetExpiry() ?? DateTimeOffset.UtcNow.Add(_minExpiry);
		DateTimeOffset dateTimeOffset2 = DateTimeOffset.UtcNow.Add(_minExpiry);
		DateTimeOffset absoluteExpiration = ((dateTimeOffset > dateTimeOffset2) ? dateTimeOffset : dateTimeOffset2);
		_responseCache.Set(key.HashBase64, value, absoluteExpiration);
	}

	public Task<bool> TryRemoveAsync(CacheKey key)
	{
		return Task.FromResult(_responseCache.Remove(key.HashBase64) != null);
	}

	public Task ClearAsync()
	{
		_responseCache.Dispose();
		_responseCache = new MemoryCache("###InMemoryCacheStore_###");
		return Task.FromResult(0);
	}
}
