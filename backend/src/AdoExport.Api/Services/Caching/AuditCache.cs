using AdoExport.Api.Configuration;
using AdoExport.Auditing;

using Microsoft.Extensions.Options;

namespace AdoExport.Api.Services.Caching;

public class AuditCache : IAuditCache
{
    private readonly IInMemoryCachingService _cachingService;

    private readonly IOptions<CacheConfiguration> _cacheOptions;

    public AuditCache(IInMemoryCachingService cachingService, IOptions<CacheConfiguration> cacheOptions)
    {
        Guard.IsNotNull(cachingService);
        Guard.IsNotNull(cacheOptions);

        _cachingService = cachingService;
        _cacheOptions = cacheOptions;
    }

    /// <inheritdoc />
    public Task SetAsync(string key, Audit data)
    {
        Guard.IsNotNullOrWhiteSpace(key);
        Guard.IsNotNull(data);

        return _cachingService.SetAsync(FormatKey(key), data, _cacheOptions.Value.CacheExpiry);
    }

    /// <inheritdoc />
    public Task<Audit?> GetAsync(string key)
    {
        Guard.IsNotNullOrWhiteSpace(key);

        return _cachingService.GetAsync<Audit>(FormatKey(key));
    }

    private static string FormatKey(string key) => $"audit-{key}";
}