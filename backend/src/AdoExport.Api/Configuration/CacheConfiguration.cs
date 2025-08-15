namespace AdoExport.Api.Configuration;

public sealed class CacheConfiguration
{
    public const string ConfigurationSectionName = "Caches";

    /// <summary>
    /// Gets the duration after which cached values expire.
    /// </summary>
    public required TimeSpan CacheExpiry { get; init; }

    /// <summary>
    /// Gets the cache type to use for storing data.
    /// </summary>
    public CacheTypes? CacheType { get; set; }
}