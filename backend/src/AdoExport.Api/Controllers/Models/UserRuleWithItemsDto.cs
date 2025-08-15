using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class UserRuleWithItemsDto : RuleDto
{
    [JsonPropertyName("items")]
    public required UserRuleDetailsDto[] Items { get; init; }
}