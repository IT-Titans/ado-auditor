using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Pats.Reasons;

public class PatIsValidForMoreThanOneOrganizationReason : PatAuditRuleReason
{
    public PatIsValidForMoreThanOneOrganizationReason(SessionTokenDto model)
        : base(model)
    {
        Severity = Severities.Warning;
    }

    public override Guid Id { get; } = Guid.Parse("CF7DCC01-8DB3-41A6-B6B9-A6B856D72C76");
}