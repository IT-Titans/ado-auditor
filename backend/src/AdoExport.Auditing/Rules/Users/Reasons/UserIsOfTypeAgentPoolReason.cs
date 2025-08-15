using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Users.Reasons;

public class UserIsOfTypeAgentPoolReason : UserAuditRuleReason
{
    public UserIsOfTypeAgentPoolReason(UserDto model)
        : base(model)
    {
        Severity = Severities.Ok;
    }

    public override Guid Id { get; } = Guid.Parse("4b5d1e5f-49f6-4151-a50e-92ce326b79c3");
}