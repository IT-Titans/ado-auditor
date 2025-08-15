namespace AdoExport.Client.AdoApi;

public interface IClientConfigProvider
{
    /// <summary>
    /// Gets the <see cref="ClientConfig" />
    /// </summary>
    /// <returns></returns>
    Task<ClientConfig> GetConfigAsync();
}