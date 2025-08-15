using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class PatDetailsUserDto
{
    [JsonPropertyName("userId")]
    public required string UserId { get; set; }

    [JsonPropertyName("userName")]
    public required string Name { get; set; }
}