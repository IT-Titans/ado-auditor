using System.Text.Json.Serialization;

#nullable disable

namespace AdoExport.Client.AdoApi.Models;

public class GetAllGroupsResponseDto
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }

    [JsonPropertyName("value")]
    public GroupDto[] Value { get; set; }
}