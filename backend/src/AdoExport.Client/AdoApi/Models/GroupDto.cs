using System.Text.Json.Serialization;

using AdoExport.Client.AdoApi.Helpers;

#nullable disable

namespace AdoExport.Client.AdoApi.Models;

public class GroupDto
{
    [JsonPropertyName("subjectKind")]
    public string SubjectKind { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("domain")]
    public string Domain { get; set; }

    [JsonPropertyName("principalName")]
    public string PrincipalName { get; set; }

    [JsonPropertyName("mailAddress")]
    public object MailAddress { get; set; }

    [JsonPropertyName("origin")]
    public string Origin { get; set; }

    [JsonPropertyName("originId")]
    public string OriginId { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("_links")]
    public LinksDto Links { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("descriptor")]
    public string Descriptor { get; set; }

    [JsonPropertyName("decodedDescriptor")]
    public string DecodedDescriptor => Descriptor.DecodeDescriptor().Payload;
}