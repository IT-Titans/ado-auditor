using AdoExport.Auditing.Rules.Pats.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Pats;

public class PersonalAccessTokenExpiryAuditRule : AuditRule, IPersonalAccessTokenAuditRule
{
    public PersonalAccessTokenExpiryAuditRule(RawAuditData rawAuditData)
        : base(rawAuditData)
    {
    }

    /// <inheritdoc />
    public RuleResult Audit(SessionTokenDto auditee)
    {
        Guard.IsNotNull(auditee);

        var validFrom = DateTime.Parse(auditee.ValidFrom).ToUniversalTime();
        var validTo = DateTime.Parse(auditee.ValidTo).ToUniversalTime();

        if (validTo - validFrom > TimeSpan.FromDays(GetDaysPerHalfYear()))
        {
            return new RuleResult()
            {
                Reason = new PatExpiresInOverHalfAYearReason(auditee),
            };
        }

        if (validTo - validFrom > TimeSpan.FromDays(GetDaysPerQuarter()))
        {
            return new RuleResult()
            {
                Reason = new PatExpiresInOverOneQuarterReason(auditee),
            };
        }

        return new RuleResult()
        {
            Reason = new PatExpiresWithinShortTimeRangeReason(auditee),
        };
    }

    private static int GetDaysPerQuarter()
    {
        return 90;
    }

    private static int GetDaysPerHalfYear()
    {
        return 180;
    }
}