using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

[JsonConverter(typeof(JsonStringEnumConverter<Categories>))]
public enum Categories
{
    [JsonStringEnumMemberName("users")]
    Users,

    [JsonStringEnumMemberName("projects")]
    Projects,

    [JsonStringEnumMemberName("pats")]
    Pats,

    [JsonStringEnumMemberName("groups")]
    Groups,
}