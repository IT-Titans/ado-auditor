namespace AdoExport.Auditing;

public static class AuditResultExtensions
{
    /// <summary>
    /// Gets the sum of all rule results with the given <paramref name="severity" />.
    /// </summary>
    /// <param name="auditResults">The collection of <see cref="AuditResult" />s.</param>
    /// <param name="severity">The severity to sum totals for.</param>
    /// <returns>The total number of affected rule results.</returns>
    public static int GetTotalRuleResultsBySeverity<TDto>(this IEnumerable<AuditResult<TDto>> auditResults, Severities severity)
    {
        ArgumentNullException.ThrowIfNull(auditResults);

        return auditResults.Sum(result => result.RuleResults.Count(r => r.Reason.Severity == severity));
    }
}