using AdoExport.Api.Configuration;
using AdoExport.Api.Services.Caching;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace AdoExport.Api.Services;

/// <summary>
/// Used to make the user credentials available for the services scoped to a single request.
/// </summary>
public sealed class ScopedUserCredentialsBearer
{
    private readonly ScopedAuditKeyBearer _auditKeyBearer;
    
    private readonly IDataProtectionProvider _dataProtectionProvider;
    
    private readonly IUserDataCache _userDataCache;

    private readonly IOptions<CacheConfiguration> _cacheOptions;

    public ScopedUserCredentialsBearer(
        ScopedAuditKeyBearer auditKeyBearer,
        IDataProtectionProvider dataProtectionProvider,
        IUserDataCache userDataCache,
        IOptions<CacheConfiguration> cacheOptions)
    {
        Guard.IsNotNull(auditKeyBearer);
        Guard.IsNotNull(dataProtectionProvider);
        Guard.IsNotNull(userDataCache);
        Guard.IsNotNull(cacheOptions);

        _auditKeyBearer = auditKeyBearer;
        _dataProtectionProvider = dataProtectionProvider;
        _userDataCache = userDataCache;
        _cacheOptions = cacheOptions;
    }

    /// <summary>
    /// Creates an async scope in which the user credentials are accessible for the <paramref name="scopedAction" />.<br />
    /// After <paramref name="scopedAction" /> completes, the user credentials are no longer accessible.
    /// </summary>
    /// <param name="organizationName">The organization name to make available during the scoped action.</param>
    /// <param name="pat">The personal access token (PAT) to make available during the scoped action.</param>
    /// <param name="scopedAction">The action that needs the user credentials.</param>
    /// <typeparam name="TResult">The <paramref name="scopedAction" />s return type.</typeparam>
    /// <returns>An awaitable task that completes when <paramref name="scopedAction" /> completes.</returns>
    public async Task<TResult> ScopedAsync<TResult>(string organizationName, string pat, Func<Task<TResult>> scopedAction)
    {
        Guard.IsNotNullOrWhiteSpace(organizationName);
        Guard.IsNotNullOrWhiteSpace(pat);
        Guard.IsNotNull(scopedAction);

        try
        {
            var protector = _dataProtectionProvider
                .CreateProtector(_auditKeyBearer.AuditKey)
                .ToTimeLimitedDataProtector();

            await _userDataCache.SetAsync(_auditKeyBearer.AuditKey, new UserCredentials(organizationName, protector.Protect(pat, _cacheOptions.Value.CacheExpiry))).ConfigureAwait(false);

            return await scopedAction().ConfigureAwait(false);
        }
        finally
        {
            await _userDataCache.InvalidateAsync(_auditKeyBearer.AuditKey).ConfigureAwait(false);
        }
    }
}