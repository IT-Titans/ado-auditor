using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class RuleWithUsersByRuleIdDto : AuditInfoBearerDto
{
    [JsonPropertyName("rule")]
    public required UserRuleWithItemsDto Rule { get; init; }
}