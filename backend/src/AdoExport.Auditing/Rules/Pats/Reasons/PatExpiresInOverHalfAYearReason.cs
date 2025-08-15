using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Pats.Reasons;

public class PatExpiresInOverHalfAYearReason : PatAuditRuleReason
{
    public PatExpiresInOverHalfAYearReason(SessionTokenDto model)
        : base(model)
    {
        Severity = Severities.Critical;
    }

    public override Guid Id { get; } = Guid.Parse("05f06a6b-5f3d-4f16-bb63-7e035c962f48");
}