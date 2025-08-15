using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Pats.Reasons;

public class PatExpiresInOverOneQuarterReason : PatAuditRuleReason
{
    public PatExpiresInOverOneQuarterReason(SessionTokenDto model)
        : base(model)
    {
        Severity = Severities.Warning;
    }

    public override Guid Id { get; } = Guid.Parse("27bd636c-6a92-4c77-9023-d91d3a0dc129");
}