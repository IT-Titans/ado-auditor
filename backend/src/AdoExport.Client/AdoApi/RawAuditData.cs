using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Client.AdoApi;

public class RawAuditData
{
    public required Dictionary<string, GroupDto[]> UserGroupMemberships { get; init; }

    public required UserDto[] AllUsers { get; init; }

    public required GetUserEntitlementsByUserEntitlementIdResponseDto[] UsersWithProjectEntitlements { get; init; }

    public required SessionTokenWithUser[] PrivateSessionTokenDetailsWithUser { get; init; }

    public required SessionTokenWithUser[] PublicSessionTokenDetailsWithUser { get; init; }
}