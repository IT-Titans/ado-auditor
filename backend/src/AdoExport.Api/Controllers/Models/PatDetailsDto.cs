using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class PatDetailsDto
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("user")]
    public required PatDetailsUserDto User { get; init; }

    [JsonPropertyName("scopes")]
    public required string[] Scopes { get; init; }

    [JsonPropertyName("createdAt")]
    public required string CreatedAt { get; init; }

    [JsonPropertyName("expiresIn")]
    public required string ExpiresIn { get; init; }

    [JsonPropertyName("affectedPatRulesCounts")]
    public required TotalSeverityCountsDto AffectedPatRulesCounts { get; init; }
}