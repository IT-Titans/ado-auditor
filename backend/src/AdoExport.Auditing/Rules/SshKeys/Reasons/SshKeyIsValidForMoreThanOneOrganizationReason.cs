using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.SshKeys.Reasons;

public class SshKeyIsValidForMoreThanOneOrganizationReason : SshKeyAuditRuleReason
{
    public SshKeyIsValidForMoreThanOneOrganizationReason(SessionTokenDto model)
        : base(model)
    {
        Severity = Severities.Warning;
    }

    public override Guid Id { get; } = Guid.Parse("0919cce4-52f0-429e-804d-72227583d3c9");
}