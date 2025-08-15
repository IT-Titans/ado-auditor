namespace AdoExport.Auditing.Rules;

public abstract class AuditRuleReason
{
    public const string ModelMetadataKey = "model";

    public AuditRuleReason(Categories category)
    {
        Category = category;
    }

    public abstract Guid Id { get; }

    public Severities Severity { get; protected set; }

    public Categories Category { get; init; }

    public Dictionary<string, object> Metadata { get; } = new();
}