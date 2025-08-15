namespace AdoExport.Auditing;

public class SummaryItem
{
    public Severities Severity { get; init; }

    public Categories Category { get; init; }

    public int AffectedCount { get; init; }

    public int TotalCount { get; init; }

    public required string Text { get; init; }
}