using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class AllUsersDto : AuditInfoBearerDto
{
    [JsonPropertyName("users")]
    public required UserEntryDto[] Users { get; init; }
}