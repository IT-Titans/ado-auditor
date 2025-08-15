using AdoExport.Auditing.Rules.Users.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Users;

public class UserPatsAuditRule : AuditRule, IUserAuditRule
{
    /// <summary>
    /// Todo: This needs to be moved into a configuration that is used in the auditor as well (<see cref="NotesOnRulesService" />).
    /// </summary>
    private const int NumberOfAllowedPats = 3;

    public UserPatsAuditRule(RawAuditData rawAuditData)
        : base(rawAuditData)
    {
    }

    /// <inheritdoc />
    public RuleResult Audit(UserDto auditee)
    {
        Guard.IsNotNull(auditee);

        var totalUserPats = RawAuditData
            .PrivateSessionTokenDetailsWithUser
            .Count(group => string.Equals(group.User?.Descriptor, auditee.Descriptor, StringComparison.OrdinalIgnoreCase));

        if (totalUserPats > NumberOfAllowedPats)
        {
            return new RuleResult()
            {
                Reason = new UserExceedsOverallPatLimitReason(auditee),
            };
        }

        return new RuleResult()
        {
            Reason = new UserIsWithinOverallPatLimitReason(auditee),
        };
    }
}