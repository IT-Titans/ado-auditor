using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class UserDetailsDto
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("userName")]
    public required string UserName { get; set; }

    [JsonPropertyName("isActive")]
    public required bool IsActive { get; set; }

    [JsonPropertyName("metaType")]
    public required string MetaType { get; set; }
}