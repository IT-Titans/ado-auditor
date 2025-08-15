using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Users.Reasons;

public class UserIsOfTypeBuildAgentReason : UserAuditRuleReason
{
    public UserIsOfTypeBuildAgentReason(UserDto model)
        : base(model)
    {
        Severity = Severities.Ok;
    }

    public override Guid Id { get; } = Guid.Parse("962a4349-6dea-4e0f-ac10-8b6ebd8a9be6");
}