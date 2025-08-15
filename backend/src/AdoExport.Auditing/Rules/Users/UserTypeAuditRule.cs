using AdoExport.Auditing.Rules.Users.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Users;

public class UserTypeAuditRule : AuditRule, IUserAuditRule
{
    public UserTypeAuditRule(RawAuditData rawAuditData)
        : base(rawAuditData)
    {
    }

    /// <inheritdoc />
    public RuleResult Audit(UserDto auditee)
    {
        Guard.IsNotNull(auditee);

        if (string.Equals(auditee.MetaType, UserDto.GuestUserMetaType, StringComparison.OrdinalIgnoreCase))
        {
            return new RuleResult()
            {
                Reason = new UserIsOfTypeGuestReason(auditee),
            };
        }

        if (auditee.MetaType is null && string.Equals(auditee.Domain, UserDto.BuildUserDomain, StringComparison.OrdinalIgnoreCase))
        {
            return new RuleResult()
            {
                Reason = new UserIsOfTypeBuildAgentReason(auditee),
            };
        }

        if (auditee.MetaType is null && string.Equals(auditee.Domain, UserDto.AgentPoolUserDomain, StringComparison.OrdinalIgnoreCase))
        {
            return new RuleResult()
            {
                Reason = new UserIsOfTypeAgentPoolReason(auditee),
            };
        }

        if (string.Equals(auditee.MetaType, UserDto.MemberUserMetaType, StringComparison.OrdinalIgnoreCase))
        {
            return new RuleResult()
            {
                Reason = new UserIsOfTypeMemberReason(auditee),
            };
        }

        return new RuleResult()
        {
            Reason = new UserIsOfTypeUnknownReason(auditee),
        };
    }
}