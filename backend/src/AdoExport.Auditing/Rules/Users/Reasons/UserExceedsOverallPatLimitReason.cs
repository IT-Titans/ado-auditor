using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Users.Reasons;

public class UserExceedsOverallPatLimitReason : UserAuditRuleReason
{
    public UserExceedsOverallPatLimitReason(UserDto model)
        : base(model)
    {
        Severity = Severities.Warning;
    }

    public override Guid Id { get; } = Guid.Parse("4B327B8A-8F83-44F7-85BD-E05B98B5FEDD");
}