using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CacheCow.Common.Helpers;

public static class HttpResponseMessageExtensions
{
	public static Task<HttpResponseMessage> ToTask(this HttpResponseMessage responseMessage)
	{
		TaskCompletionSource<HttpResponseMessage> taskCompletionSource = new TaskCompletionSource<HttpResponseMessage>();
		taskCompletionSource.SetResult(responseMessage);
		return taskCompletionSource.Task;
	}

	public static DateTimeOffset? GetExpiry(this HttpResponseMessage response)
	{
		if (response.Headers.CacheControl != null && response.Headers.CacheControl.MaxAge.HasValue)
		{
			return DateTimeOffset.UtcNow.Add(response.Headers.CacheControl.MaxAge.Value);
		}
		if (response.Content == null || !response.Content.Headers.Expires.HasValue)
		{
			return null;
		}
		return response.Content.Headers.Expires.Value;
	}

	public static async Task WhatEnsureSuccessShouldHaveBeen(this HttpResponseMessage response)
	{
		if (!response.IsSuccessStatusCode)
		{
			if (response.Content != null)
			{
				string arg = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
				throw new HttpRequestException($"Status: {response.StatusCode}\r\n Error: {arg}");
			}
			response.EnsureSuccessStatusCode();
		}
	}
}
