using System.Text.Json;

using StackExchange.Redis;

namespace AdoExport.Api.Services.Caching;

/// <summary>
/// An implementation of <see cref="ICachingService" /> using Redis directly.
/// </summary>
public class RedisCachingService : ICachingService
{
    private readonly ILogger<RedisCachingService> _logger;

    private readonly IConnectionMultiplexer _redis;

    public RedisCachingService(
        ILogger<RedisCachingService> logger,
        IConnectionMultiplexer redis)
    {
        Guard.IsNotNull(logger);
        _logger = logger;

        Guard.IsNotNull(redis);
        _redis = redis;
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T? value, TimeSpan expiry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var db = _redis.GetDatabase();

        await db.StringSetAsync(
            key: key,
            value: JsonSerializer.Serialize(value),
            expiry: expiry).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var db = _redis.GetDatabase();

        if (await db.KeyExistsAsync(key).ConfigureAwait(false) == false)
        {
            return default(T);
        }

        var value = await db.StringGetAsync(key).ConfigureAwait(false);

        return JsonSerializer.Deserialize<T>((string)value!);
    }

    /// <inheritdoc />
    public async Task InvalidateAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var db = _redis.GetDatabase();

        await db.KeyDeleteAsync(key).ConfigureAwait(false);
    }
}