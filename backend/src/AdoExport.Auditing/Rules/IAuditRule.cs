namespace AdoExport.Auditing.Rules;

public interface IAuditRule<T>
{
    RuleResult Audit(T auditee);
}