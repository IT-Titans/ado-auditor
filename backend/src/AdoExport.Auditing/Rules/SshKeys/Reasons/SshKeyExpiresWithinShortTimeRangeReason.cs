using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.SshKeys.Reasons;

public class SshKeyExpiresWithinShortTimeRangeReason : SshKeyAuditRuleReason
{
    public SshKeyExpiresWithinShortTimeRangeReason(SessionTokenDto model)
        : base(model)
    {
        Severity = Severities.Ok;
    }

    public override Guid Id { get; } = Guid.Parse("de414e9c-ae7e-4498-a12f-68ce12ec4efc");
}