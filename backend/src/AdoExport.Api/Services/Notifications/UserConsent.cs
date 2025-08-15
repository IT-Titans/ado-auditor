using System.Text.Json.Serialization;

using AdoExport.Api.Services.UserConsent;

namespace AdoExport.Api.Services.Notifications;

/// <summary>
/// Base class for storing user consent providing minimum required data.
/// </summary>
public abstract class UserConsent
{
    /// <summary>
    /// Gets or sets the user's identifier.
    /// </summary>
    [JsonPropertyName("userIdentifier")]
    public required string Identifier { get; init; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the user gave consent.
    /// </summary>
    [JsonPropertyName("consentTimestamp")]
    public required DateTime TimeOfConsent { get; init; }

    /// <summary>
    /// Gets or sets how / for what the user gave consent.
    /// </summary>
    [JsonPropertyName("consentOrigin")]
    public abstract ConsentOrigins ConsentOrigin { get; }
}