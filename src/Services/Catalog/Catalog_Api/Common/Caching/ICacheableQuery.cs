namespace Catalog_Api.Common.Caching
{
    public interface ICacheableQuery
    {
        string CacheKey { get; }
        TimeSpan Expiration { get; }
    }
}
