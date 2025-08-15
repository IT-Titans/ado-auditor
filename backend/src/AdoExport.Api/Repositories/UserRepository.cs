using AdoExport.Api.Auditing;
using AdoExport.Api.Controllers;
using AdoExport.Api.Services;
using AdoExport.Api.Services.Caching;
using AdoExport.Auditing;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ScopedAuditKeyBearer _auditKeyBearer;

    private readonly ICreateAuditDataCache _auditDataCache;

    private CreateAuditData? _auditData;

    public UserRepository(
        ScopedAuditKeyBearer auditKeyBearer,
        ICreateAuditDataCache auditDataCache)
    {
        _auditKeyBearer = auditKeyBearer;
        _auditDataCache = auditDataCache;
    }

    /// <inheritdoc />
    public async Task<UserDto?> GetUserByDescriptorAsync(string userDescriptor)
    {
        var exportData = await GetExportDataAsync().ConfigureAwait(false);

        return exportData
            .AllUsers
            .FirstOrDefault(user => string.Equals(user.Descriptor, userDescriptor, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public async Task<UserDto?> GetUserByUserIdAsync(string userId)
    {
        var exportData = await GetExportDataAsync().ConfigureAwait(false);

        return (exportData
            .PrivateSessionTokenDetailsWithUser
            .Concat(exportData.PublicSessionTokenDetailsWithUser))
            .FirstOrDefault(token => token.SessionToken.UserId == userId)?
            .User;
    }

    /// <inheritdoc />
    public async Task<UserDto[]> GetAllUsersAsync()
    {
        var exportData = await GetExportDataAsync().ConfigureAwait(false);

        return exportData.AllUsers;
    }

    /// <inheritdoc />
    public async Task<AccessLevelDto?> GetAccessLevelByUserDescriptorAsync(string userDescriptor)
    {
        var exportData = await GetExportDataAsync().ConfigureAwait(false);

        return exportData
            .UsersWithProjectEntitlements
            .Where(entitlement => string.Equals(entitlement.User.Descriptor, userDescriptor, StringComparison.OrdinalIgnoreCase))
            .ToArray()
            .FirstOrDefault()?
            .AccessLevel;
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