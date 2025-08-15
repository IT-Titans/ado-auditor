using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class RuleDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("scope")]
    public required Categories Scope { get; init; }

    [JsonPropertyName("severity")]
    public required Severities Severity { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("recommendation")]
    public required string Recommendation { get; init; }

    [JsonPropertyName("totalCounts")]
    public required int TotalCounts { get; init; }
}