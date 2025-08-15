using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

/// <code>
/// [
///     {
///         "severity": "critical | warning | ok"
///         "affectedCount": 1,
///         "totalCount": 4,
///         "text": "Devs have admin rights"
///     }
/// ]
/// </code>
public class AuditSummaryCategoryItemDto
{
    [JsonPropertyName("severity")]
    public Severities Severity { get; init; }

    [JsonPropertyName("affectedCount")]
    public int AffectedCount { get; init; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; init; }

    [JsonPropertyName("text")]
    public required string Text { get; init; }
}