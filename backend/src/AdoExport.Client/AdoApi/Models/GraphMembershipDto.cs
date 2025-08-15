using System.Text.Json.Serialization;

#nullable disable

namespace AdoExport.Client.AdoApi.Models;

public class GraphMembershipDto
{
    [JsonPropertyName("containerDescriptor")]
    public string ContainerDescriptor { get; set; }

    [JsonPropertyName("memberDescriptor")]
    public string MemberDescriptor { get; set; }
}