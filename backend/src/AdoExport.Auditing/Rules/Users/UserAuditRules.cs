using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Users;

public class UserAuditRules : AuditRules<UserDto>
{
    public UserAuditRules(RawAuditData rawAuditData)
    {
        Rules =
        [
            new UserTypeAuditRule(rawAuditData),
            new UserPatsAuditRule(rawAuditData),
            new UserOrganizationAdminAuditRule(rawAuditData),
        ];
    }
}