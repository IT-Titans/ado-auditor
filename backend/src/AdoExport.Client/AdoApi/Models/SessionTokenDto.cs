using System.Text.Json.Serialization;

namespace AdoExport.Client.AdoApi.Models;

#nullable disable

public class SessionTokenDto
{
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; }

    [JsonPropertyName("accessId")]
    public string AccessId { get; set; }

    [JsonPropertyName("authorizationId")]
    public string AuthorizationId { get; set; }

    [JsonPropertyName("hostAuthorizationId")]
    public string HostAuthorizationId { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("validFrom")]
    public string ValidFrom { get; set; }

    [JsonPropertyName("validTo")]
    public string ValidTo { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    [JsonPropertyName("targetAccounts")]
    public List<string>? TargetAccounts { get; set; }

    [JsonPropertyName("token")]
    public object Token { get; set; }

    [JsonPropertyName("alternateToken")]
    public object AlternateToken { get; set; }

    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; set; }

    [JsonPropertyName("publicData")]
    public object PublicData { get; set; }

    [JsonPropertyName("source")]
    public object Source { get; set; }

    [JsonPropertyName("claims")]
    public object Claims { get; set; }
}