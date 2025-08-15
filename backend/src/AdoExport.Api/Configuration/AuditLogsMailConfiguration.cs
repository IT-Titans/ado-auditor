namespace AdoExport.Api.Configuration;

public class AuditLogsMailConfiguration
{
    public const string ConfigurationSectionName = "AuditLogs";

    /// <summary>
    /// Gets or sets the sender address from which audit logs mails are sent.
    /// </summary>
    public required string Sender { get; init; }

    /// <summary>
    /// Gets or sets the receiver address to which audit logs mails are sent.
    /// </summary>
    public required string Receiver { get; init; }
}