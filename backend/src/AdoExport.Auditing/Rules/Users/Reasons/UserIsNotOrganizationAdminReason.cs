using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Users.Reasons;

public class UserIsNotOrganizationAdminReason : UserAuditRuleReason
{
    public UserIsNotOrganizationAdminReason(UserDto model)
        : base(model)
    {
        Severity = Severities.Ok;
    }

    public override Guid Id { get; } = Guid.Parse("14cf0a21-aa67-4a11-8cea-abf0bfaec4b1");
}