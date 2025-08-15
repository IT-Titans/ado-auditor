namespace AdoExport.Api.Configuration;

public class ErrorReportsMailConfiguration
{
    public const string ConfigurationSectionName = "ErrorReports";

    /// <summary>
    /// Gets or sets the sender address from which user consent mails are sent.
    /// </summary>
    public required string Sender { get; init; }

    /// <summary>
    /// Gets or sets the receiver address to which user consent mails are sent.
    /// </summary>
    public required string Receiver { get; init; }
}