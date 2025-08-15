using AdoExport.Auditing.Rules.Users.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Users;

public class UserOrganizationAdminAuditRule : AuditRule, IUserAuditRule
{
    public UserOrganizationAdminAuditRule(RawAuditData rawAuditData)
        : base(rawAuditData)
    {
    }

    /// <inheritdoc />
    public RuleResult Audit(UserDto auditee)
    {
        Guard.IsNotNull(auditee);

        var organizationAdmins = RawAuditData
            .UsersWithProjectEntitlements
            .Select(userEntitlement =>
                new
                {
                    User = userEntitlement.User,
                    Groups = (RawAuditData.UserGroupMemberships.TryGetValue(userEntitlement.User.Descriptor,
                            out var groups)
                            ? groups
                            : [])
                        .ToArray(),
                }
            )
            .Where(userEntitlementMapping => userEntitlementMapping
                .Groups
                .Any(group => group.PrincipalName.EndsWith("\\Project Collection Administrators")))
            .ToArray();

        if (organizationAdmins.Any(admin => admin.User.Descriptor == auditee.Descriptor))
        {
            return new RuleResult()
            {
                Reason = new UserIsOrganizationAdminReason(auditee),
            };
        }

        return new RuleResult()
        {
            Reason = new UserIsNotOrganizationAdminReason(auditee),
        };
    }
}