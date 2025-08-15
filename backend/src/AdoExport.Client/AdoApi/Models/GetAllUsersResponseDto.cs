using System.Text.Json.Serialization;

#nullable disable

namespace AdoExport.Client.AdoApi.Models;

public class GetAllUsersResponseDto
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }

    [JsonPropertyName("value")]
    public UserDto[] Value { get; set; }
}