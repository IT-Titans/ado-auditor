using System.Text.Json.Serialization;

using AdoExport.Api.Services.UserConsent;

namespace AdoExport.Api.Services.Notifications;

public sealed class ContactFormUserConsent : UserConsent
{
    /// <inheritdoc />
    [JsonPropertyName("consentOrigin")]
    public override ConsentOrigins ConsentOrigin => ConsentOrigins.ContactForm;
}