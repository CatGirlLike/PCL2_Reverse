namespace CacheCow.Common;

public enum CacheValidationStatus
{
	None,
	GetIfModifiedSince,
	GetIfNoneMatch,
	PutPatchDeleteIfUnModifiedSince,
	PutPatchDeleteIfMatch
}
