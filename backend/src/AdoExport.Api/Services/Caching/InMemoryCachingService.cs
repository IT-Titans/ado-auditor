using AdoExport.Api.Configuration;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AdoExport.Api.Services.Caching;

/// <summary>
/// An implementation of <see cref="ICachingService" /> using Redis directly.
/// </summary>
public class InMemoryCachingService : ICachingService, IInMemoryCachingService
{
    private readonly ILogger<InMemoryCachingService> _logger;

    private readonly IOptions<CacheConfiguration> _cacheOptions;

    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public InMemoryCachingService(ILogger<InMemoryCachingService> logger, IOptions<CacheConfiguration> cacheOptions)
    {
        Guard.IsNotNull(logger);
        Guard.IsNotNull(cacheOptions);

        _logger = logger;
        _cacheOptions = cacheOptions;
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T? value, TimeSpan expiry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _cache.Set(key, value, _cacheOptions.Value.CacheExpiry);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _cache.TryGetValue(key, out T? data);

        return Task.FromResult(data);
    }

    /// <inheritdoc />
    public Task InvalidateAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _cache.Remove(key);

        return Task.CompletedTask;
    }
}