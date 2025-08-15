using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Pats.Reasons;

public class PatIsValidForAllOrganizationsReason : PatAuditRuleReason
{
    public PatIsValidForAllOrganizationsReason(SessionTokenDto model)
        : base(model)
    {
        Severity = Severities.Critical;
    }

    public override Guid Id { get; } = Guid.Parse("e6a47ea8-34f1-43c5-b50c-df0ebda16cfc");
}