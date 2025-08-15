using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Pats.Reasons;

public class PatHasAppScopeReason : PatAuditRuleReason
{
    public PatHasAppScopeReason(SessionTokenDto model)
        : base(model)
    {
        Severity = Severities.Critical;
    }

    public override Guid Id { get; } = Guid.Parse("5a5adb71-e8ac-40d3-8956-15e6465be028");
}