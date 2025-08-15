using System.Text.Json.Serialization;

#nullable disable

namespace AdoExport.Client.AdoApi.Models;

public class ProjectEntitlementsDto
{
    [JsonPropertyName("projectRef")]
    public ProjectReferenceDto ProjectReference { get; set; }

    [JsonPropertyName("group")]
    public ProjectEntitlementsGroupDto Group { get; set; }

    [JsonPropertyName("projectPermissionInherited")]
    public string ProjectPermissionInherited { get; set; }

    [JsonPropertyName("assignmentSource")]
    public string AssignmentSource { get; set; }
}