using AdoExport.Api.Configuration;

using Azure;
using Azure.Communication.Email;

using Microsoft.Extensions.Options;

namespace AdoExport.Api.Services.Notifications;

/// <summary>
/// Sends mails using the azure communication services.
/// </summary>
public sealed class AzureCommunicationsSendMailService : ISendMailService
{
    private readonly ILogger<AzureCommunicationsSendMailService> _logger;

    private readonly EmailClient _emailClient;

    public AzureCommunicationsSendMailService(
        ILogger<AzureCommunicationsSendMailService> logger,
        IOptions<AzureCommunicationServiceConfiguration> configuration)
    {
        Guard.IsNotNull(logger);
        _logger = logger;

        Guard.IsNotNull(configuration);
        _emailClient = new EmailClient(configuration.Value.ConnectionString);
    }

    /// <inheritdoc />
    public async Task SendMailAsync(string sender, string receiver, string subject, string body)
    {
        var emailContent = new EmailContent(subject)
        {
            PlainText = body
        };

        var emailRecipients = new EmailRecipients([new EmailAddress(receiver)]);
        var emailMessage = new EmailMessage(sender, emailRecipients, emailContent);

        var result = await _emailClient.SendAsync(WaitUntil.Completed, emailMessage).ConfigureAwait(false);
        if (result.Value.Status == EmailSendStatus.Succeeded)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Email sent successfully");
            }
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("Sending email with subject '{Subject}' failed: '{status}'", subject, result.Value.Status);
            }
        }
    }
}