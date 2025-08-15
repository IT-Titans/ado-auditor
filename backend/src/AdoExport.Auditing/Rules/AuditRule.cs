using AdoExport.Client.AdoApi;

namespace AdoExport.Auditing.Rules;

public abstract class AuditRule
{
    protected AuditRule(RawAuditData rawAuditData)
    {
        Guard.IsNotNull(rawAuditData);

        RawAuditData = rawAuditData;
    }

    protected RawAuditData RawAuditData { get; }
}