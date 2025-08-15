namespace AdoExport.Api.Configuration;

public class ContactMailConfiguration
{
    public const string ConfigurationSectionName = "Contact";

    /// <summary>
    /// Gets or sets the sender address from which contact mails are sent.
    /// </summary>
    public required string Sender { get; init; }

    /// <summary>
    /// Gets or sets the receiver address to which contact mails are sent.
    /// </summary>
    public required string Receiver { get; init; }
}