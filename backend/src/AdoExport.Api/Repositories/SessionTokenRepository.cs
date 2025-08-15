using AdoExport.Api.Auditing;
using AdoExport.Api.Controllers;
using AdoExport.Api.Services;
using AdoExport.Api.Services.Caching;
using AdoExport.Auditing;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Api.Repositories;

public class SessionTokenRepository : ISessionTokenRepository
{
    private readonly ScopedAuditKeyBearer _auditKeyBearer;

    private readonly ICreateAuditDataCache _auditDataCache;

    private CreateAuditData? _auditData;

    public SessionTokenRepository(
        ScopedAuditKeyBearer auditKeyBearer,
        ICreateAuditDataCache auditDataCache)
    {
        _auditKeyBearer = auditKeyBearer;
        _auditDataCache = auditDataCache;
    }

    /// <inheritdoc />
    public async Task<SessionTokenDto[]> GetAllSessionTokensByUserDescriptorAsync(string userDescriptor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userDescriptor);

        var getPatsTask = GetPatsByUserDescriptorAsync(userDescriptor);
        var getSshKeysTask = GetSshKeysByUserDescriptorAsync(userDescriptor);

        await Task.WhenAll([
            getPatsTask,
            getSshKeysTask,
        ]).ConfigureAwait(false);

        return getPatsTask.Result
            .Concat(getSshKeysTask.Result)
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<SessionTokenDto[]> GetPatsByUserDescriptorAsync(string userDescriptor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userDescriptor);

        var exportData = await GetExportDataAsync().ConfigureAwait(false);

        return exportData
            .PrivateSessionTokenDetailsWithUser
            .Where(token => string.Equals(token.User.Descriptor, userDescriptor, StringComparison.OrdinalIgnoreCase))
            .Select(token => token.SessionToken)
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<SessionTokenDto[]> GetSshKeysByUserDescriptorAsync(string userDescriptor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userDescriptor);

        var exportData = await GetExportDataAsync().ConfigureAwait(false);

        return exportData
            .PublicSessionTokenDetailsWithUser
            .Where(token => string.Equals(token.User.Descriptor, userDescriptor, StringComparison.OrdinalIgnoreCase))
            .Select(token => token.SessionToken)
            .ToArray();
    }

    private async Task<RawAuditData> GetExportDataAsync()
    {
        if (_auditData is null)
        {
            _auditData = (await _auditDataCache.GetAsync(_auditKeyBearer.AuditKey).ConfigureAwait(false))
                         ?? throw new InvalidOperationException("Audit data cannot be null.");
        }

        return _auditData.ExportedData ?? throw new InvalidOperationException("Raw audit data cannot be null.");
    }
}