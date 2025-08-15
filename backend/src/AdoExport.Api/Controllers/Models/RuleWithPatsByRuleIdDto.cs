using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class RuleWithPatsByRuleIdDto : AuditInfoBearerDto
{
    [JsonPropertyName("rule")]
    public required PatRuleWithItemsDto Rule { get; init; }
}