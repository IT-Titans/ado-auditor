using AdoExport.Api.Configuration;
using AdoExport.Auditing;

using Microsoft.Extensions.Options;

namespace AdoExport.Api.Services.Caching;

public class CreateAuditDataCache : ICreateAuditDataCache
{
    private readonly ICachingService _cachingService;

    private readonly IOptions<CacheConfiguration> _cacheOptions;

    public CreateAuditDataCache(ICachingService cachingService, IOptions<CacheConfiguration> cacheOptions)
    {
        Guard.IsNotNull(cachingService);
        Guard.IsNotNull(cacheOptions);

        _cachingService = cachingService;
        _cacheOptions = cacheOptions;
    }

    /// <inheritdoc />
    public Task SetAsync(string key, CreateAuditData data)
    {
        Guard.IsNotNullOrWhiteSpace(key);
        Guard.IsNotNull(data);

        return _cachingService.SetAsync(FormatKey(key), data, _cacheOptions.Value.CacheExpiry);
    }

    /// <inheritdoc />
    public Task<CreateAuditData?> GetAsync(string key)
    {
        Guard.IsNotNullOrWhiteSpace(key);

        return _cachingService.GetAsync<CreateAuditData>(FormatKey(key));
    }

    private static string FormatKey(string key) => $"create-audit-data-{key}";
}