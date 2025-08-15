using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

/// <summary>
/// Serves as a base class for audit info bearing DTOs.
/// </summary>
public abstract class AuditInfoBearerDto
{
    [JsonPropertyName("organizationName")]
    public required string OrganizationName { get; init; }

    /// <summary>
    /// The UTC timestamp the audit was requested to be created at, formatted as sortable time string.
    /// </summary>
    /// <remarks>See: https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#the-sortable-s-format-specifier</remarks>
    [JsonPropertyName("requestedAt")]
    public required string RequestedAtDateString { get; init; }

    /// <summary>
    /// The UTC timestamp the audit was created at, formatted as sortable time string.
    /// </summary>
    /// <remarks>See: https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#the-sortable-s-format-specifier</remarks>
    [JsonPropertyName("createdAt")]
    public required string CreatedAtDateString { get; init; }

    /// <summary>
    /// The UTC timestamp after which an audit expires and is no longer valid, formatted as sortable time string.
    /// </summary>
    /// <remarks>See: https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#the-sortable-s-format-specifier</remarks>
    [JsonPropertyName("validUntil")]
    public required string ValidUntilDateString { get; init; }
}