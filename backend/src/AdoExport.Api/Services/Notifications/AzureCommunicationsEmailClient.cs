using AdoExport.Api.Configuration;

using Azure;
using Azure.Communication.Email;

using Microsoft.Extensions.Options;

namespace AdoExport.Api.Services.Notifications;

/// <summary>
/// Sends mails through the Azure Communication Services SDK.
/// </summary>
public sealed class AzureCommunicationsEmailClient : IEmailClient
{
    private readonly EmailClient _emailClient;

    public AzureCommunicationsEmailClient(IOptions<AzureCommunicationServiceConfiguration> configuration)
        : this(CreateClient(configuration))
    {
    }

    /// <summary>
    /// For tests, which hand in a client whose transport never leaves the process.
    /// </summary>
    internal AzureCommunicationsEmailClient(EmailClient emailClient)
    {
        Guard.IsNotNull(emailClient);

        _emailClient = emailClient;
    }

    /// <inheritdoc />
    public async Task<EmailSendStatus> SendAsync(string sender, string receiver, string subject, string plainTextBody)
    {
        var emailContent = new EmailContent(subject)
        {
            PlainText = plainTextBody
        };

        var emailRecipients = new EmailRecipients([new EmailAddress(receiver)]);
        var emailMessage = new EmailMessage(sender, emailRecipients, emailContent);

        var operation = await _emailClient.SendAsync(WaitUntil.Completed, emailMessage).ConfigureAwait(false);

        return operation.Value.Status;
    }

    private static EmailClient CreateClient(IOptions<AzureCommunicationServiceConfiguration> configuration)
    {
        Guard.IsNotNull(configuration);

        return new EmailClient(configuration.Value.ConnectionString);
    }
}
