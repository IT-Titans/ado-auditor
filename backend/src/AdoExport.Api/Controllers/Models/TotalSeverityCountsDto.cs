using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class TotalSeverityCountsDto : IHasTotalSeverities
{
    /// <inheritdoc />
    [JsonPropertyName("totalCriticals")]
    public required int TotalCriticals { get; init; }

    /// <inheritdoc />
    [JsonPropertyName("totalWarnings")]
    public required int TotalWarnings { get; init; }

    /// <inheritdoc />
    [JsonPropertyName("totalOks")]
    public required int TotalOks { get; init; }
}