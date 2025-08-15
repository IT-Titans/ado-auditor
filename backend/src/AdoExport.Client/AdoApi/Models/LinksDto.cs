using System.Text.Json.Serialization;

#nullable disable

namespace AdoExport.Client.AdoApi.Models;

public class LinksDto
{
    [JsonPropertyName("self")]
    public LinkDto Self { get; set; }

    [JsonPropertyName("memberships")]
    public LinkDto Memberships { get; set; }

    [JsonPropertyName("membershipState")]
    public LinkDto MembershipState { get; set; }

    [JsonPropertyName("storageKey")]
    public LinkDto StorageKey { get; set; }
}