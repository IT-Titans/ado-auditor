using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Pats.Reasons;

public class PatIsValidForOnlyOneOrganizationReason : PatAuditRuleReason
{
    public PatIsValidForOnlyOneOrganizationReason(SessionTokenDto model)
        : base(model)
    {
        Severity = Severities.Ok;
    }

    public override Guid Id { get; } = Guid.Parse("F7084C80-0444-4525-BF8C-F8C58C07B026");
}