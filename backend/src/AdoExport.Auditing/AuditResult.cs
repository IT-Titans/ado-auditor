using AdoExport.Auditing.Rules;

namespace AdoExport.Auditing;

public class AuditResult<T> : AuditResult
{
    public required T Auditee { get; init; }
}

public class AuditResult
{
    public required RuleResult[] RuleResults { get; init; }
}