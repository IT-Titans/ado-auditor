using AdoExport.Api.Services;
using AdoExport.Api.Services.Caching;
using AdoExport.Client.AdoApi;

using Microsoft.AspNetCore.DataProtection;

namespace AdoExport.Api.Configuration;

public sealed class ClientConfigProvider : IClientConfigProvider
{
    private readonly ScopedAuditKeyBearer _keyBearer;

    private readonly IUserDataCache _userDataCache;

    private readonly IDataProtectionProvider _dataProtectionProvider;

    private readonly Lazy<Task<ClientConfig>> _configTask;

    public ClientConfigProvider(ScopedAuditKeyBearer keyBearer, IUserDataCache userDataCache, IDataProtectionProvider dataProtectionProvider)
    {
        Guard.IsNotNull(keyBearer);
        Guard.IsNotNull(userDataCache);
        Guard.IsNotNull(dataProtectionProvider);

        _keyBearer = keyBearer;
        _userDataCache = userDataCache;
        _dataProtectionProvider = dataProtectionProvider;

        _configTask = new Lazy<Task<ClientConfig>>(InitializeAsync);
    }

    /// <inheritdoc />
    public Task<ClientConfig> GetConfigAsync() => _configTask.Value;

    private async Task<ClientConfig> InitializeAsync()
    {
        var userData = await _userDataCache.GetAsync(_keyBearer.AuditKey).ConfigureAwait(false);
        if (userData is null)
        {
            throw new InvalidOperationException("No user data has been configured");
        }

        var protector = _dataProtectionProvider
            .CreateProtector(_keyBearer.AuditKey)
            .ToTimeLimitedDataProtector();

        return new ClientConfig()
        {
            OrganizationName = userData.Organization,
            Pat = protector.Unprotect(userData.Token),
        };
    }
}