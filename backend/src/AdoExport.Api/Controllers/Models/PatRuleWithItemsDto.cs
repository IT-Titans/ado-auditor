using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class PatRuleWithItemsDto : RuleDto
{
    [JsonPropertyName("items")]
    public required PatDetailsDto[] Items { get; init; }
}