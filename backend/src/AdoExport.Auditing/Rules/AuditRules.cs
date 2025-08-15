namespace AdoExport.Auditing.Rules;

public abstract class AuditRules<TAuditee>
{
    protected IAuditRule<TAuditee>[] Rules { get; init; } = [];

    public AuditResult<TAuditee> Audit(TAuditee auditee)
    {
        var results = Rules
            .Select(rule => rule.Audit(auditee))
            .ToArray();

        return new AuditResult<TAuditee>()
        {
            Auditee = auditee,
            RuleResults = results,
        };
    }
}