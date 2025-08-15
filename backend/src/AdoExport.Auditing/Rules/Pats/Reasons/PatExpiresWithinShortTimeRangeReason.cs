using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Pats.Reasons;

public class PatExpiresWithinShortTimeRangeReason : PatAuditRuleReason
{
    public PatExpiresWithinShortTimeRangeReason(SessionTokenDto model)
        : base(model)
    {
        Severity = Severities.Ok;
    }

    public override Guid Id { get; } = Guid.Parse("1529f5ce-6264-4296-8d54-605e68836317");
}