using AdoExport.Auditing.Rules.SshKeys.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.SshKeys;

public class SshKeyValidForAllOrganizationsAuditRule : AuditRule, ISshKeyAuditRule
{
    public SshKeyValidForAllOrganizationsAuditRule(RawAuditData rawAuditData)
        : base(rawAuditData)
    {
    }

    public RuleResult Audit(SessionTokenDto auditee)
    {
        if (auditee.TargetAccounts is null || auditee.TargetAccounts.Count == 0)
        {
            return new RuleResult()
            {
                Reason = new SshKeyIsValidForAllOrganizationsReason(auditee),
            };
        }

        if (auditee.TargetAccounts.Count > 1)
        {
            return new RuleResult()
            {
                Reason = new SshKeyIsValidForMoreThanOneOrganizationReason(auditee),
            };
        }

        return new RuleResult()
        {
            Reason = new SshKeyIsValidForOnlyOneOrganizationReason(auditee),
        };
    }
}