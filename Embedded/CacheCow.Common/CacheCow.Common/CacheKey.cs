using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CacheCow.Common;

public class CacheKey
{
	private readonly string _resourceUri;

	private readonly string _toString;

	private readonly string _routePattern;

	private readonly byte[] _hash;

	private readonly string _hashBase64;

	private string _domain;

	private const string CacheKeyFormat = "{0}-{1}";

	public string ResourceUri => _resourceUri;

	public byte[] Hash => _hash;

	public string HashBase64 => _hashBase64;

	public CacheKey(string resourceUri, IEnumerable<string> headerValues = null)
	{
		_toString = string.Format("{0}-{1}", resourceUri, string.Join("-", headerValues));
		using (SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider())
		{
			_hash = sHA1CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(_toString));
		}
		_hashBase64 = Convert.ToBase64String(_hash);
		_resourceUri = resourceUri;
	}

	public override string ToString()
	{
		return _toString;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is CacheKey cacheKey))
		{
			return false;
		}
		return ToString() == cacheKey.ToString();
	}

	public override int GetHashCode()
	{
		return _toString.GetHashCode();
	}
}
