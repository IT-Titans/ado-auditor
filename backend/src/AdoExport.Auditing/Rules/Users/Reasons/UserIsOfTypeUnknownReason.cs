using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Users.Reasons;

public class UserIsOfTypeUnknownReason : UserAuditRuleReason
{
    public UserIsOfTypeUnknownReason(UserDto model)
        : base(model)
    {
        Severity = Severities.Critical;
    }

    public override Guid Id { get; } = Guid.Parse("d1ce9fd4-50c1-4899-8a5d-4120acbf3d54");
}