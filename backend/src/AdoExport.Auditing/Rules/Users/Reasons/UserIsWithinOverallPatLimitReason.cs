using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Users.Reasons;

public class UserIsWithinOverallPatLimitReason : UserAuditRuleReason
{
    public UserIsWithinOverallPatLimitReason(UserDto model)
        : base(model)
    {
        Severity = Severities.Ok;
    }

    public override Guid Id { get; } = Guid.Parse("37c18e21-67ed-4abf-9471-73cbcf743138");
}