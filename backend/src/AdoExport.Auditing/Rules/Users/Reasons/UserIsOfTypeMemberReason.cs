using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Users.Reasons;

public class UserIsOfTypeMemberReason : UserAuditRuleReason
{
    public UserIsOfTypeMemberReason(UserDto model)
        : base(model)
    {
        Severity = Severities.Ok;
    }

    public override Guid Id { get; } = Guid.Parse("498a578a-110e-4cd6-ba80-ba641171420a");
}