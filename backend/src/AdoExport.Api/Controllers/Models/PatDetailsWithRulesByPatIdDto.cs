using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class PatDetailsWithRulesByPatIdDto : AuditInfoBearerDto
{
    [JsonPropertyName("pat")]
    public required PatDetailsDto Pat { get; init; }

    [JsonPropertyName("rules")]
    public required RuleDto[] Rules { get; init; }
}