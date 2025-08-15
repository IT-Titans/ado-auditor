using System.Text.Json.Serialization;

#nullable disable

namespace AdoExport.Client.AdoApi.Models;

public class UserEntitlementsDto
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("user")] public UserDto User { get; set; }
}