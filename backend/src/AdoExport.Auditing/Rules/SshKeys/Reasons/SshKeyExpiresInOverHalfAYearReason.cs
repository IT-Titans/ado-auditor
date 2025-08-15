using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.SshKeys.Reasons;

public class SshKeyExpiresInOverHalfAYearReason : SshKeyAuditRuleReason
{
    public SshKeyExpiresInOverHalfAYearReason(SessionTokenDto model)
        : base(model)
    {
        Severity = Severities.Warning;
    }

    public override Guid Id { get; } = Guid.Parse("20922ad1-012e-4c6a-8b1d-4c712a819679");
}