using System.Text.Json.Serialization;

#nullable disable

namespace AdoExport.Client.AdoApi.Models;

public class GetSubjectMembershipsResponseDto
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("value")]
    public GraphMembershipDto[] Value { get; set; }
}