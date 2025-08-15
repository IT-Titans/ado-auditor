using AdoExport.Auditing.Rules.Pats.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Pats;

public class PersonalAccessTokenScopesAuditRule : AuditRule, IPersonalAccessTokenAuditRule
{
    public PersonalAccessTokenScopesAuditRule(RawAuditData rawAuditData)
        : base(rawAuditData)
    {
    }

    /// <inheritdoc />
    public RuleResult Audit(SessionTokenDto auditee)
    {
        Guard.IsNotNull(auditee);

        if (auditee.Scope == "app_token")
        {
            return new RuleResult()
            {
                Reason = new PatHasAppScopeReason(auditee),
            };
        }

        return new RuleResult()
        {
            Reason = new PatHasLimitedScopeReason(auditee),
        };
    }
}