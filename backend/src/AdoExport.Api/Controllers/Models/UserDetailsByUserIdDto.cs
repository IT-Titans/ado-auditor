using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class UserDetailsByUserIdDto : AuditInfoBearerDto
{
    [JsonPropertyName("user")]
    public required UserDetailsDto User { get; init; }

    [JsonPropertyName("patsSummary")]
    public required TotalSeverityCountsDto PatsSummary { get; init; }

    [JsonPropertyName("pats")]
    public required PatDetailsDto[] Pats { get; init; }

    [JsonPropertyName("rules")]
    public required RuleDto[] Rules { get; init; }
}