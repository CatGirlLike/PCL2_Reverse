namespace CacheCow.Client;

public class CacheStoreSettings
{
	public long TotalQuota { get; set; }

	public long PerDomainQuota { get; set; }

	public CacheStoreSettings()
	{
		TotalQuota = long.MaxValue;
		PerDomainQuota = 52428800L;
	}
}
