using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class PatRulesBySeverityDto : AuditInfoBearerDto
{
    [JsonPropertyName("rules")]
    public required RuleDto[] Rules { get; init; }
}