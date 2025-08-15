using AdoExport.Api.Controllers.Models;

namespace AdoExport.Api.Reports.Templates.Components;

public class SeverityCounts
{
    public required Severities Severity { get; init; }

    public required int Count { get; init; }
}