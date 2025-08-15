namespace AdoExport.Api.Configuration;

public class UserConsentMailConfiguration
{
    public const string ConfigurationSectionName = "UserConsent";

    /// <summary>
    /// Gets or sets the sender address from which user consent mails are sent.
    /// </summary>
    public required string Sender { get; init; }

    /// <summary>
    /// Gets or sets the receiver address to which user consent mails are sent.
    /// </summary>
    public required string Receiver { get; init; }
}