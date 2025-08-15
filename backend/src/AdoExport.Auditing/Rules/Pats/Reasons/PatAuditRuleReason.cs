using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Pats.Reasons;

public abstract class PatAuditRuleReason : AuditRuleReason
{
    protected PatAuditRuleReason(SessionTokenDto model)
        : base(Categories.Pats)
    {
        Metadata.Add(ModelMetadataKey, model);
    }
}