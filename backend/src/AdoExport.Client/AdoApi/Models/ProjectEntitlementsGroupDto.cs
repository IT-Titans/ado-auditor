using System.Text.Json.Serialization;

#nullable disable

namespace AdoExport.Client.AdoApi.Models;

public class ProjectEntitlementsGroupDto
{
    [JsonPropertyName("groupType")]
    public string GroupType { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }
}