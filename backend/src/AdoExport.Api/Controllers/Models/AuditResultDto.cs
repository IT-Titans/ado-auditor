using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class AuditResultDto : AuditInfoBearerDto
{
    [JsonPropertyName("totalCount")]
    public required int TotalIssuesCount { get; init; }

    [JsonPropertyName("criticalCount")]
    public required int CriticalIssuesCount { get; init; }

    [JsonPropertyName("warningCount")]
    public required int WarningIssuesCount { get; init; }

    [JsonPropertyName("okCount")]
    public required int OkIssuesCount { get; init; }

    [JsonPropertyName("summaries")]
    public required AuditSummaryCategoryDto[] Summaries { get; init; }
}