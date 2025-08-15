using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Users.Reasons;

public class UserIsOfTypeGuestReason : UserAuditRuleReason
{
    public UserIsOfTypeGuestReason(UserDto model)
        : base(model)
    {
        Severity = Severities.Warning;
    }

    public override Guid Id { get; } = Guid.Parse("459ddf63-91b0-4722-89f7-548f19b36799");
}