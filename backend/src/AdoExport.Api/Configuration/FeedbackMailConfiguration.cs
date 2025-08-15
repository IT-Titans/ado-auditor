namespace AdoExport.Api.Configuration;

public class FeedbackMailConfiguration
{
    public const string ConfigurationSectionName = "Feedback";

    /// <summary>
    /// Gets or sets the sender address from which feedback mails are sent.
    /// </summary>
    public required string Sender { get; init; }

    /// <summary>
    /// Gets or sets the receiver address to which feedback mails are sent.
    /// </summary>
    public required string Receiver { get; init; }
}