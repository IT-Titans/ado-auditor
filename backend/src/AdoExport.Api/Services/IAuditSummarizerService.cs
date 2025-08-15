using AdoExport.Api.Auditing;
using AdoExport.Auditing;

namespace AdoExport.Api.Services;

public interface IAuditSummarizerService
{
    /// <summary>
    /// Gets a list of summaries for the given <paramref name="audit" />.
    /// </summary>
    /// <param name="audit">The audit to create summaries for.</param>
    /// <returns>A list of summaries describing the audit.</returns>
    IEnumerable<SummaryItem> Summarize(Audit audit);
}