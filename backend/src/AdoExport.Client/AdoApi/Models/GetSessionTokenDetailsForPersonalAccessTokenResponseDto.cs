using System.Text.Json.Serialization;

namespace AdoExport.Client.AdoApi.Models;

#nullable disable

public class GetSessionTokenDetailsForPersonalAccessTokenResponseDto
{
    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }

    [JsonPropertyName("value")]
    public SessionTokenDto[] Value { get; set; }
}