using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Pats.Reasons;

public class PatHasLimitedScopeReason : PatAuditRuleReason
{
    public PatHasLimitedScopeReason(SessionTokenDto model)
        : base(model)
    {
        Severity = Severities.Ok;
    }

    public override Guid Id { get; } = Guid.Parse("0b705033-2e32-4ce3-b606-7dceb31aee96");
}