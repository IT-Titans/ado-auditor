using AdoExport.Auditing.Rules.Pats.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Pats;

public class PersonalAccessTokenValidForAllOrganizationsAuditRule : AuditRule, IPersonalAccessTokenAuditRule
{
    public PersonalAccessTokenValidForAllOrganizationsAuditRule(RawAuditData rawAuditData)
        : base(rawAuditData)
    {
    }

    /// <inheritdoc />
    public RuleResult Audit(SessionTokenDto auditee)
    {
        Guard.IsNotNull(auditee);

        if (auditee.TargetAccounts is null || auditee.TargetAccounts.Count == 0)
        {
            return new RuleResult()
            {
                Reason = new PatIsValidForAllOrganizationsReason(auditee),
            };
        }

        if (auditee.TargetAccounts.Count > 1)
        {
            return new RuleResult()
            {
                Reason = new PatIsValidForMoreThanOneOrganizationReason(auditee),
            };
        }

        return new RuleResult()
        {
            Reason = new PatIsValidForOnlyOneOrganizationReason(auditee),
        };
    }
}