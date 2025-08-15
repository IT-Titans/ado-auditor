using System.Text.Json.Serialization;

#nullable disable

namespace AdoExport.Client.AdoApi.Models;

public class GetUserEntitlementsByUserEntitlementIdResponseDto
{
    [JsonPropertyName("user")]
    public UserDto User { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("accessLevel")]
    public AccessLevelDto AccessLevel { get; set; }

    [JsonPropertyName("projectEntitlements")]
    public ProjectEntitlementsDto[] ProjectEntitlements { get; set; }
}