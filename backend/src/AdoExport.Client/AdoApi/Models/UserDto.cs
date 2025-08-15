using System.Text.Json.Serialization;

using AdoExport.Client.AdoApi.Helpers;

#nullable disable

namespace AdoExport.Client.AdoApi.Models;

public class UserDto
{
    public const string MemberUserMetaType = "member";

    public const string GuestUserMetaType = "guest";

    public const string BuildUserDomain = "build";

    public const string AgentPoolUserDomain = "agentpool";

    [JsonPropertyName("subjectKind")]
    public string SubjectKind { get; set; }

    [JsonPropertyName("metaType")]
    public string? MetaType { get; set; }

    [JsonPropertyName("directoryAlias")]
    public string DirectoryAlias { get; set; }

    [JsonPropertyName("domain")]
    public string Domain { get; set; }

    [JsonPropertyName("principalName")]
    public string PrincipalName { get; set; }

    [JsonPropertyName("mailAddress")]
    public string MailAddress { get; set; }

    [JsonPropertyName("origin")]
    public string Origin { get; set; }

    /// <summary>
    ///  This seems to be the user id.
    /// </summary>
    [JsonPropertyName("originId")]
    public string OriginId { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("descriptor")]
    public string Descriptor { get; set; }

    [JsonPropertyName("decodedDescriptor")]
    public string DecodedDescriptor => Descriptor.DecodeDescriptor().Payload;
}