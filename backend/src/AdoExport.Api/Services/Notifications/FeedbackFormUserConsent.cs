using System.Text.Json.Serialization;

using AdoExport.Api.Services.UserConsent;

namespace AdoExport.Api.Services.Notifications;

public sealed class FeedbackFormUserConsent : UserConsent
{
    /// <inheritdoc />
    [JsonPropertyName("consentOrigin")]
    public override ConsentOrigins ConsentOrigin => ConsentOrigins.FeedbackForm;

    /// <summary>
    /// Gets or sets the audit identifier.<br />
    /// Set when consent is given in the context of an audit; otherwise null.
    /// </summary>
    [JsonPropertyName("auditIdentifier")]
    public required string? AuditId { get; init; }
}