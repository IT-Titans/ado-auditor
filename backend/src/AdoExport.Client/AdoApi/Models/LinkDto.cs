using System.Text.Json.Serialization;

#nullable disable

namespace AdoExport.Client.AdoApi.Models;

public class LinkDto
{
    [JsonPropertyName("href")]
    public string Href { get; set; }
}