using System.Text.Json.Serialization;

using AdoExport.Api.Services.UserConsent;

namespace AdoExport.Api.Services.Notifications;

public sealed class StartAuditFormUserConsent : UserConsent
{
    /// <inheritdoc />
    [JsonPropertyName("consentOrigin")]
    public override ConsentOrigins ConsentOrigin => ConsentOrigins.StartAuditForm;

    /// <summary>
    /// Gets or sets the audit identifier.<br />
    /// Set when consent is given in the context of an audit; otherwise null.
    /// </summary>
    [JsonPropertyName("auditIdentifier")]
    public required string? AuditId { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the user gave consent to anonymous audit logging.
    /// </summary>
    [JsonPropertyName("consentToAnonymousLogging")]
    public required bool ConsentForAnonymousLogging { get; init; }
}