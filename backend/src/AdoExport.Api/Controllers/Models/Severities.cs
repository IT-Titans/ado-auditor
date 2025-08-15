using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

[JsonConverter(typeof(JsonStringEnumConverter<Severities>))]
public enum Severities
{
    [JsonStringEnumMemberName("critical")]
    Critical,

    [JsonStringEnumMemberName("warning")]
    Warning,

    [JsonStringEnumMemberName("ok")]
    Ok
}