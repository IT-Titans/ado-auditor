using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.SshKeys.Reasons;

public class SshKeyIsValidForOnlyOneOrganizationReason : SshKeyAuditRuleReason
{
    public SshKeyIsValidForOnlyOneOrganizationReason(SessionTokenDto model)
        : base(model)
    {
        Severity = Severities.Ok;
    }

    public override Guid Id { get; } = Guid.Parse("41616316-59e1-4198-98de-90247be45073");
}