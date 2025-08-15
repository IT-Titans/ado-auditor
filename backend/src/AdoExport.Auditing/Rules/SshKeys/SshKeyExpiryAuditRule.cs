using AdoExport.Auditing.Rules.SshKeys.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.SshKeys;

public class SshKeyExpiryAuditRule : AuditRule, ISshKeyAuditRule
{
    private const int DaysPerYear = 365;

    public SshKeyExpiryAuditRule(RawAuditData rawAuditData)
        : base(rawAuditData)
    {
    }

    public RuleResult Audit(SessionTokenDto auditee)
    {
        if (DateTime.Parse(auditee.ValidTo) - DateTime.Parse(auditee.ValidFrom) >= TimeSpan.FromDays(DaysPerYear))
        {
            return new RuleResult()
            {
                Reason = new SshKeyExpiresInOverAYearReason(auditee),
            };
        }

        if (DateTime.Parse(auditee.ValidTo) - DateTime.Parse(auditee.ValidFrom) >= TimeSpan.FromDays(DaysPerYear / 2))
        {
            return new RuleResult()
            {
                Reason = new SshKeyExpiresInOverHalfAYearReason(auditee),
            };
        }

        return new RuleResult()
        {
            Reason = new SshKeyExpiresWithinShortTimeRangeReason(auditee),
        };
    }
}