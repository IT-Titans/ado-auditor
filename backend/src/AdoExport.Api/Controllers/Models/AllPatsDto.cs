using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class AllPatsDto : AuditInfoBearerDto
{
    [JsonPropertyName("pats")]
    public required PatDetailsDto[] Pats { get; init; }
}