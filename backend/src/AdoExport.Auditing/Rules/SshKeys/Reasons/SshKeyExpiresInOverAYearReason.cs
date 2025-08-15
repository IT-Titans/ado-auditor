using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.SshKeys.Reasons;

public class SshKeyExpiresInOverAYearReason : SshKeyAuditRuleReason
{
    public SshKeyExpiresInOverAYearReason(SessionTokenDto model)
        : base(model)
    {
        Severity = Severities.Critical;
    }

    public override Guid Id { get; } = Guid.Parse("cb5666b5-fc99-4994-9ce3-b464bff65cf7");
}