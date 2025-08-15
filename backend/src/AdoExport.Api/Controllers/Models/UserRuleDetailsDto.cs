using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class UserRuleDetailsDto
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("userName")]
    public required string UserName { get; init; }

    [JsonPropertyName("metaType")]
    public required string MetaType { get; init; }

    [JsonPropertyName("affectedUserRulesCounts")]
    public required TotalSeverityCountsDto AffectedUserRulesCounts { get; init; }
}