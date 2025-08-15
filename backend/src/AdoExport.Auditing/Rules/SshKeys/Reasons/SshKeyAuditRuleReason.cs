using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.SshKeys.Reasons;

public abstract class SshKeyAuditRuleReason : AuditRuleReason
{
    protected SshKeyAuditRuleReason(SessionTokenDto model)
        : base(Categories.Pats)
    {
        Metadata.Add(ModelMetadataKey, model);
    }
}