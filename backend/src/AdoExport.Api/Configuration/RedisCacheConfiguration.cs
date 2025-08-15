namespace AdoExport.Api.Configuration;

public sealed class RedisCacheConfiguration
{
    public const string ConfigurationSectionName = "RedisCache";

    /// <summary>
    /// Gets the redis configuration / connection string.
    /// </summary>
    public required string Configuration { get; init; }
}