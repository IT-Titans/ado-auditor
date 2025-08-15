using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class UserRulesBySeverityDto : AuditInfoBearerDto
{
    [JsonPropertyName("rules")]
    public required RuleDto[] Rules { get; init; }
}