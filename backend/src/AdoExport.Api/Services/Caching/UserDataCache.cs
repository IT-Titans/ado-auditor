using AdoExport.Api.Configuration;

using Microsoft.Extensions.Options;

namespace AdoExport.Api.Services.Caching;

public class UserDataCache : IUserDataCache
{
    private readonly IInMemoryCachingService _cachingService;

    private readonly IOptions<CacheConfiguration> _cacheOptions;

    public UserDataCache(IInMemoryCachingService cachingService, IOptions<CacheConfiguration> cacheOptions)
    {
        Guard.IsNotNull(cachingService);
        Guard.IsNotNull(cacheOptions);

        _cachingService = cachingService;
        _cacheOptions = cacheOptions;
    }

    /// <inheritdoc />
    public Task SetAsync(string key, UserCredentials data)
    {
        Guard.IsNotNullOrWhiteSpace(key);
        Guard.IsNotNull(data);

        return _cachingService.SetAsync(FormatKey(key), data, _cacheOptions.Value.CacheExpiry);
    }

    /// <inheritdoc />
    public Task<UserCredentials?> GetAsync(string key)
    {
        Guard.IsNotNullOrWhiteSpace(key);

        return _cachingService.GetAsync<UserCredentials>(FormatKey(key));
    }

    /// <inheritdoc />
    public Task InvalidateAsync(string key)
    {
        Guard.IsNotNullOrWhiteSpace(key);

        return _cachingService.InvalidateAsync(FormatKey(key));
    }

    private static string FormatKey(string key) => $"userdata-{key}";
}