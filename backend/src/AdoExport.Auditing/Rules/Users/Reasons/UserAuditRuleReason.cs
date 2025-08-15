using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Users.Reasons;

public abstract class UserAuditRuleReason : AuditRuleReason
{
    protected UserAuditRuleReason(UserDto model)
        : base(Categories.Users)
    {
        Metadata.Add(ModelMetadataKey, model);
    }
}