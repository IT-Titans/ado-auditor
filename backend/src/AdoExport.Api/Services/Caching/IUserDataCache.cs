namespace AdoExport.Api.Services.Caching;

public interface IUserDataCache
{
    /// <summary>
    /// Associates and stores the given <paramref name="data" /> with the given <paramref name="key" />. 
    /// </summary>
    /// <param name="key">The key identifying the data.</param>
    /// <param name="data">The data to store.</param>
    /// <returns>An awaitable task.</returns>
    Task SetAsync(string key, UserCredentials data);

    /// <summary>
    /// Gets the <see cref="UserCredentials" /> associated with the given key or null if none could be found.
    /// </summary>
    /// <param name="key">The key identifying the data.</param>
    /// <returns>An awaitable task providing the associated <see cref="UserCredentials" /> or null if none could be found.</returns>
    Task<UserCredentials?> GetAsync(string key);

    /// <summary>
    /// Invalidates user data associated with the given <paramref name="key" />.<br />
    /// After invalidation, the data is no longer available.
    /// </summary>
    /// <param name="key">The key identifying the data.</param>
    /// <returns>An awaitable task.</returns>
    Task InvalidateAsync(string key);
}