using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Users.Reasons;

public class UserIsOrganizationAdminReason : UserAuditRuleReason
{
    public UserIsOrganizationAdminReason(UserDto model)
        : base(model)
    {
        Severity = Severities.Warning;
    }

    public override Guid Id { get; } = Guid.Parse("d9883edd-8181-4acb-9e13-b44e04d2ca7d");
}