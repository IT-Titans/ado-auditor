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
        Guard.IsNotNull(auditee);

        // Compare instants, not local wall-clock values: subtracting two local DateTimes ignores
        // daylight saving changes and shifts the lifetime by an hour across them.
        var validFrom = DateTime.Parse(auditee.ValidFrom).ToUniversalTime();
        var validTo = DateTime.Parse(auditee.ValidTo).ToUniversalTime();
        var lifetime = validTo - validFrom;

        if (lifetime >= TimeSpan.FromDays(DaysPerYear))
        {
            return new RuleResult()
            {
                Reason = new SshKeyExpiresInOverAYearReason(auditee),
            };
        }

        if (lifetime >= TimeSpan.FromDays(DaysPerYear / 2))
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