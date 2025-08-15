using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.SshKeys.Reasons;

public class SshKeyIsValidForAllOrganizationsReason : SshKeyAuditRuleReason
{
    public SshKeyIsValidForAllOrganizationsReason(SessionTokenDto model)
        : base(model)
    {
        Severity = Severities.Critical;
    }

    public override Guid Id { get; } = Guid.Parse("3bd706aa-f35d-4c68-8a76-90398a0c5860");
}