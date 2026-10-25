using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CacheCow.Client.Headers;
using CacheCow.Client.Internal;
using CacheCow.Common;
using CacheCow.Common.Helpers;

namespace CacheCow.Client;

public class CachingHandler : DelegatingHandler
{
	private readonly ICacheStore _cacheStore;

	private Func<HttpRequestMessage, bool> _ignoreRequestRules;

	private bool _disposeCacheStore;

	private bool _disposeVaryStore;

	private static HttpStatusCode[] _cacheableStatuses;

	public IVaryHeaderStore VaryHeaderStore { get; set; }

	public string[] DefaultVaryHeaders { get; set; }

	public string[] StarVaryHeaders { get; set; }

	public bool UseConditionalPutPatchDelete { get; set; }

	public bool MustRevalidateByDefault { get; set; }

	public Func<HttpResponseMessage, ResponseValidationResult> ResponseValidator { get; set; }

	public Action<HttpResponseMessage> ResponseStoragePreparationRules { get; set; }

	public bool DoNotEmitCacheCowHeader { get; set; }

	public static Action<Exception> IgnoreExceptionPolicy { get; private set; }

	public CachingHandler()
		: this(new InMemoryCacheStore())
	{
		_disposeCacheStore = true;
	}

	public CachingHandler(ICacheStore cacheStore)
		: this(cacheStore, new InMemoryVaryHeaderStore())
	{
		_disposeVaryStore = true;
	}

	public CachingHandler(ICacheStore cacheStore, IVaryHeaderStore varyHeaderStore)
	{
		_cacheStore = cacheStore;
		UseConditionalPutPatchDelete = true;
		MustRevalidateByDefault = true;
		VaryHeaderStore = varyHeaderStore;
		DefaultVaryHeaders = new string[1] { "Accept" };
		ResponseValidator = delegate(HttpResponseMessage response)
		{
			if (!response.StatusCode.IsIn(_cacheableStatuses))
			{
				return ResponseValidationResult.NotCacheable;
			}
			if (!response.IsSuccessStatusCode || response.Headers.CacheControl == null || response.Headers.CacheControl.NoStore)
			{
				return ResponseValidationResult.NotCacheable;
			}
			if (!response.Headers.Date.HasValue)
			{
				TraceWriter.WriteLine("Response date is NULL", TraceLevel.Warning);
			}
			response.Headers.Date = response.Headers.Date ?? DateTimeOffset.UtcNow;
			DateTimeOffset? date = response.Headers.Date;
			TimeSpan ts = TimeSpan.Zero;
			if (response.Headers.Age.HasValue)
			{
				ts = response.Headers.Age.Value;
			}
			TraceWriter.WriteLine($"CachedResponse date was => {date} - compared to UTC.Now => {DateTimeOffset.UtcNow}", TraceLevel.Verbose);
			if (response.Content == null)
			{
				return ResponseValidationResult.NotCacheable;
			}
			if (!response.Headers.CacheControl.MaxAge.HasValue && !response.Headers.CacheControl.SharedMaxAge.HasValue && !response.Content.Headers.Expires.HasValue)
			{
				return ResponseValidationResult.NotCacheable;
			}
			if (response.Headers.CacheControl.NoCache)
			{
				return ResponseValidationResult.MustRevalidate;
			}
			if (response.RequestMessage?.Headers?.CacheControl != null && response.RequestMessage.Headers.CacheControl.NoCache)
			{
				return ResponseValidationResult.MustRevalidate;
			}
			if (response.Headers.CacheControl.MaxAge.HasValue && DateTimeOffset.UtcNow > response.Headers.Date.Value.Add(response.Headers.CacheControl.MaxAge.Value.Subtract(ts)))
			{
				if (!response.Headers.CacheControl.ShouldRevalidate(MustRevalidateByDefault))
				{
					return ResponseValidationResult.Stale;
				}
				return ResponseValidationResult.MustRevalidate;
			}
			if (response.Headers.CacheControl.SharedMaxAge.HasValue && DateTimeOffset.UtcNow > response.Headers.Date.Value.Add(response.Headers.CacheControl.SharedMaxAge.Value.Subtract(ts)))
			{
				if (!response.Headers.CacheControl.ShouldRevalidate(MustRevalidateByDefault))
				{
					return ResponseValidationResult.Stale;
				}
				return ResponseValidationResult.MustRevalidate;
			}
			return (response.Content.Headers.Expires.HasValue && response.Content.Headers.Expires < DateTimeOffset.UtcNow) ? ((!response.Headers.CacheControl.ShouldRevalidate(MustRevalidateByDefault)) ? ResponseValidationResult.Stale : ResponseValidationResult.MustRevalidate) : ResponseValidationResult.OK;
		};
		_ignoreRequestRules = delegate(HttpRequestMessage request)
		{
			if (request.Method.IsCacheIgnorable())
			{
				return true;
			}
			return (request.Headers.CacheControl != null && request.Headers.CacheControl.NoStore) ? true : false;
		};
		ResponseStoragePreparationRules = delegate(HttpResponseMessage response)
		{
			if (response.Content.Headers.Expires.HasValue && (response.Headers.CacheControl.MaxAge.HasValue || response.Headers.CacheControl.SharedMaxAge.HasValue))
			{
				response.Content.Headers.Expires = null;
			}
		};
	}

	static CachingHandler()
	{
		_cacheableStatuses = new HttpStatusCode[6]
		{
			HttpStatusCode.OK,
			HttpStatusCode.NonAuthoritativeInformation,
			HttpStatusCode.PartialContent,
			HttpStatusCode.MultipleChoices,
			HttpStatusCode.MovedPermanently,
			HttpStatusCode.Gone
		};
		IgnoreExceptionPolicy = delegate
		{
		};
	}

	internal static bool? IsFreshOrStaleAcceptable(HttpResponseMessage cachedResponse, HttpRequestMessage request)
	{
		TimeSpan timeSpan = TimeSpan.Zero;
		TimeSpan value = TimeSpan.Zero;
		if (cachedResponse == null)
		{
			throw new ArgumentNullException("cachedResponse");
		}
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (cachedResponse.Content == null)
		{
			return null;
		}
		if (cachedResponse.Headers.Age.HasValue)
		{
			value = cachedResponse.Headers.Age.Value;
		}
		DateTimeOffset? dateTimeOffset = cachedResponse.Headers.Date ?? cachedResponse.Content.Headers.LastModified;
		if (!dateTimeOffset.HasValue)
		{
			return null;
		}
		if (cachedResponse.Headers.CacheControl == null)
		{
			return null;
		}
		if (cachedResponse.Content.Headers.Expires.HasValue)
		{
			timeSpan = DateTimeOffset.Now.Subtract(cachedResponse.Content.Headers.Expires.Value);
		}
		if (cachedResponse.Headers.CacheControl.MaxAge.HasValue)
		{
			timeSpan = DateTimeOffset.Now.Subtract(dateTimeOffset.Value.Subtract(value).Add(cachedResponse.Headers.CacheControl.MaxAge.Value));
		}
		if (request.Headers.CacheControl == null)
		{
			return timeSpan < TimeSpan.Zero;
		}
		if (request.Headers.CacheControl.MinFresh.HasValue)
		{
			return -timeSpan > request.Headers.CacheControl.MinFresh.Value;
		}
		if (request.Headers.CacheControl.MaxStale)
		{
			return true;
		}
		if (request.Headers.CacheControl.MaxStaleLimit.HasValue)
		{
			return timeSpan < request.Headers.CacheControl.MaxStaleLimit.Value;
		}
		if (request.Headers.CacheControl.MaxAge.HasValue)
		{
			return dateTimeOffset.Value.Subtract(value).Add(request.Headers.CacheControl.MaxAge.Value) > DateTimeOffset.Now;
		}
		return false;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		CacheCowHeader cacheCowHeader = new CacheCowHeader();
		string uri = request.RequestUri.ToString();
		List<KeyValuePair<string, IEnumerable<string>>> originalHeaders = request.Headers.ToList();
		TraceWriter.WriteLine("{0} - Starting SendAsync", TraceLevel.Verbose, uri);
		if (_ignoreRequestRules(request))
		{
			return await base.SendAsync(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (!VaryHeaderStore.TryGetValue(uri, out var varyHeaders))
		{
			varyHeaders = DefaultVaryHeaders;
		}
		CacheKey cacheKey2 = new CacheKey(uri, originalHeaders.Where((KeyValuePair<string, IEnumerable<string>> x) => varyHeaders.Any((string y) => y.Equals(x.Key, StringComparison.CurrentCultureIgnoreCase))).SelectMany((KeyValuePair<string, IEnumerable<string>> z) => z.Value));
		ResponseValidationResult validationResultForCachedResponse = ResponseValidationResult.NotExist;
		TraceWriter.WriteLine("{0} - Before TryGetValue", TraceLevel.Verbose, uri);
		HttpResponseMessage cachedResponse = await _cacheStore.GetValueAsync(cacheKey2).ConfigureAwait(continueOnCapturedContext: false);
		cacheCowHeader.DidNotExist = cachedResponse == null;
		TraceWriter.WriteLine("{0} - After TryGetValue: DidNotExist => {1}", TraceLevel.Verbose, uri, cacheCowHeader.DidNotExist);
		if (!cacheCowHeader.DidNotExist.Value)
		{
			TraceWriter.WriteLine("{0} - Existed in the cache. CacheControl Headers => {1}", TraceLevel.Verbose, uri, cachedResponse.Headers.CacheControl.ToString());
			cachedResponse.RequestMessage = request;
			validationResultForCachedResponse = ResponseValidator(cachedResponse);
		}
		TraceWriter.WriteLine("{0} - After ResponseValidator => {1}", TraceLevel.Verbose, request.RequestUri, validationResultForCachedResponse);
		if (request.Method.IsPutPatchOrDelete() && validationResultForCachedResponse.IsIn(ResponseValidationResult.OK, ResponseValidationResult.MustRevalidate))
		{
			ApplyPutPatchDeleteValidationHeaders(request, cacheCowHeader, cachedResponse);
			return await base.SendAsync(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		switch (validationResultForCachedResponse)
		{
		case ResponseValidationResult.OK:
			cacheCowHeader.RetrievedFromCache = true;
			if (!DoNotEmitCacheCowHeader)
			{
				cachedResponse.AddCacheCowHeader(cacheCowHeader);
			}
			return cachedResponse;
		case ResponseValidationResult.Stale:
		{
			cacheCowHeader.WasStale = true;
			bool? flag = IsFreshOrStaleAcceptable(cachedResponse, request);
			if (flag.HasValue && flag.Value)
			{
				if (!DoNotEmitCacheCowHeader)
				{
					cachedResponse.AddCacheCowHeader(cacheCowHeader);
				}
				return cachedResponse;
			}
			validationResultForCachedResponse = ResponseValidationResult.MustRevalidate;
			break;
		}
		case ResponseValidationResult.MustRevalidate:
			ApplyGetCacheValidationHeaders(request, cacheCowHeader, cachedResponse);
			break;
		}
		HttpResponseMessage serverResponse = await base.SendAsync(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (serverResponse.Content != null)
		{
			TraceWriter.WriteLine($"Content Size: {serverResponse.Content.Headers.ContentLength}", TraceLevel.Verbose);
			if (serverResponse.Content.Headers.ContentType == null)
			{
				serverResponse.Content.Headers.Add("Content-Type", "application/octet-stream");
			}
		}
		TraceWriter.WriteLine("{0} - After getting response", TraceLevel.Verbose, uri);
		if (request.Method != HttpMethod.Get)
		{
			return serverResponse;
		}
		if (validationResultForCachedResponse == ResponseValidationResult.MustRevalidate && serverResponse.StatusCode == HttpStatusCode.NotModified)
		{
			TraceWriter.WriteLine("{0} - Got 304 from the server and ResponseValidationResult.MustRevalidate", TraceLevel.Verbose, uri);
			cachedResponse.RequestMessage = request;
			cacheCowHeader.RetrievedFromCache = true;
			TraceWriter.WriteLine("{0} - NotModified", TraceLevel.Verbose, uri);
			await UpdateCachedResponseAsync(cacheKey2, cachedResponse, serverResponse, _cacheStore).ConfigureAwait(continueOnCapturedContext: false);
			ConsumeAndDisposeResponse(serverResponse);
			if (!DoNotEmitCacheCowHeader)
			{
				cachedResponse.AddCacheCowHeader(cacheCowHeader).CopyOtherCacheCowHeaders(serverResponse);
			}
			return cachedResponse;
		}
		ResponseValidationResult responseValidationResult = ResponseValidator(serverResponse);
		if (responseValidationResult == ResponseValidationResult.OK || responseValidationResult == ResponseValidationResult.MustRevalidate)
		{
			TraceWriter.WriteLine("{0} - ResponseValidationResult.OK or MustRevalidate", TraceLevel.Verbose, uri);
			ResponseStoragePreparationRules(serverResponse);
			if (serverResponse.Headers.Vary?.Any() ?? false)
			{
				varyHeaders = serverResponse.Headers.Vary.Select((string x) => x).ToArray();
				if (!VaryHeaderStore.TryGetValue(uri, out var _))
				{
					VaryHeaderStore.AddOrUpdate(uri, varyHeaders);
				}
			}
			cacheKey2 = new CacheKey(uri, originalHeaders.Where((KeyValuePair<string, IEnumerable<string>> x) => varyHeaders.Any((string y) => y.Equals(x.Key, StringComparison.CurrentCultureIgnoreCase))).SelectMany((KeyValuePair<string, IEnumerable<string>> z) => z.Value));
			CheckForCacheCowHeader(serverResponse);
			await _cacheStore.AddOrUpdateAsync(cacheKey2, serverResponse).ConfigureAwait(continueOnCapturedContext: false);
			TraceWriter.WriteLine("{0} - After AddOrUpdate", TraceLevel.Verbose, uri);
		}
		else
		{
			TraceWriter.WriteLine("{0} - ResponseValidationResult. Other", TraceLevel.Verbose, uri);
			TraceWriter.WriteLine("{0} - Before TryRemove", TraceLevel.Verbose, uri);
			await _cacheStore.TryRemoveAsync(cacheKey2);
			TraceWriter.WriteLine("{0} - After TryRemoveAsync", TraceLevel.Verbose, uri);
			cacheCowHeader.NotCacheable = true;
		}
		TraceWriter.WriteLine("{0} - Before returning response", TraceLevel.Verbose, request.RequestUri.ToString());
		if (!DoNotEmitCacheCowHeader)
		{
			serverResponse.AddCacheCowHeader(cacheCowHeader);
		}
		return serverResponse;
	}

	private void ApplyPutPatchDeleteValidationHeaders(HttpRequestMessage request, CacheCowHeader cacheCowHeader, HttpResponseMessage cachedResponse)
	{
		if (UseConditionalPutPatchDelete)
		{
			cacheCowHeader.CacheValidationApplied = true;
			if (cachedResponse.Headers.ETag != null)
			{
				request.Headers.Add("If-Match", cachedResponse.Headers.ETag.ToString());
			}
			else if (cachedResponse.Content.Headers.LastModified.HasValue)
			{
				request.Headers.Add("If-Unmodified-Since", cachedResponse.Content.Headers.LastModified.Value.ToString("r"));
			}
		}
	}

	internal static async Task UpdateCachedResponseAsync(CacheKey cacheKey, HttpResponseMessage cachedResponse, HttpResponseMessage serverResponse, ICacheStore store)
	{
		TraceWriter.WriteLine("CachingHandler.UpdateCachedResponseAsync - response: " + serverResponse.Headers.ToString(), TraceLevel.Verbose);
		if (serverResponse.Headers.CacheControl != null && !serverResponse.Headers.CacheControl.NoCache)
		{
			TraceWriter.WriteLine("CachingHandler.UpdateCachedResponseAsync - CacheControl: " + serverResponse.Headers.CacheControl.ToString(), TraceLevel.Verbose);
			cachedResponse.Headers.CacheControl = serverResponse.Headers.CacheControl;
		}
		else
		{
			TraceWriter.WriteLine("CachingHandler.UpdateCachedResponseAsync - CacheControl missing from server. Applying sliding expiration. Date => " + DateTimeOffset.UtcNow.ToString(), TraceLevel.Verbose);
		}
		cachedResponse.Headers.Date = DateTimeOffset.UtcNow;
		CheckForCacheCowHeader(cachedResponse);
		await store.AddOrUpdateAsync(cacheKey, cachedResponse).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static void CheckForCacheCowHeader(HttpResponseMessage responseMessage)
	{
		if (responseMessage.Headers.GetCacheCowHeader() != null)
		{
			TraceWriter.WriteLine("!!WARNING!! response stored with CacheCowHeader!!", TraceLevel.Warning);
		}
	}

	private static void ApplyGetCacheValidationHeaders(HttpRequestMessage request, CacheCowHeader cacheCowHeader, HttpResponseMessage cachedResponse)
	{
		cacheCowHeader.CacheValidationApplied = true;
		cacheCowHeader.WasStale = true;
		if (cachedResponse.Headers.ETag != null)
		{
			request.Headers.Add("If-None-Match", cachedResponse.Headers.ETag.ToString());
		}
		else if (cachedResponse.Content.Headers.LastModified.HasValue)
		{
			request.Headers.Add("If-Modified-Since", cachedResponse.Content.Headers.LastModified.Value.ToString("r"));
		}
	}

	private void ConsumeAndDisposeResponse(HttpResponseMessage response)
	{
		response.Dispose();
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			if (VaryHeaderStore != null && _disposeVaryStore)
			{
				VaryHeaderStore.Dispose();
			}
			if (_cacheStore != null && _disposeCacheStore)
			{
				_cacheStore.Dispose();
			}
		}
	}
}
