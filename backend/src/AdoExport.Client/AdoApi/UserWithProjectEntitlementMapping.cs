using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Client.AdoApi;

public class UserWithProjectEntitlementMapping
{
    public required UserDto User { get; set; }

public required GetUserEntitlementsByUserEntitlementIdResponseDto Entitlement { get; set; }
}