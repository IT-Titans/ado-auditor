using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

/// <code>
/// {
///     "scope": "pats",
///     "totalCount": 10,
///     "criticalCount": 5,
///     "warningCount": 1,
///     "okCount": 4,
///     "items":
///     [
///         {
///             // <see cref="AuditSummaryCategoryItemDto" />
///         }
///     ]
/// }
/// </code>
public class AuditSummaryCategoryDto
{
    [JsonPropertyName("scope")]
    public Categories Category { get; init; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; init; }

    [JsonPropertyName("criticalCount")]
    public required int CriticalIssuesCount { get; init; }

    [JsonPropertyName("warningCount")]
    public required int WarningIssuesCount { get; init; }

    [JsonPropertyName("okCount")]
    public required int OkIssuesCount { get; init; }

    [JsonPropertyName("items")]
    public required AuditSummaryCategoryItemDto[] Items { get; init; }
}