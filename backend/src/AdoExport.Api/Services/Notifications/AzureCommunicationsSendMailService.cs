using Azure;
using Azure.Communication.Email;

using FluentResults;

namespace AdoExport.Api.Services.Notifications;

/// <summary>
/// Sends mails using the azure communication services.
/// </summary>
public sealed class AzureCommunicationsSendMailService : ISendMailService
{
    private readonly ILogger<AzureCommunicationsSendMailService> _logger;

    private readonly IEmailClient _emailClient;

    public AzureCommunicationsSendMailService(
        ILogger<AzureCommunicationsSendMailService> logger,
        IEmailClient emailClient)
    {
        Guard.IsNotNull(logger);
        _logger = logger;

        Guard.IsNotNull(emailClient);
        _emailClient = emailClient;
    }

    /// <inheritdoc />
    public async Task<Result> SendMailAsync(string sender, string receiver, string subject, string body)
    {
        EmailSendStatus status;
        try
        {
            status = await _emailClient.SendAsync(sender, receiver, subject, body).ConfigureAwait(false);
        }
        catch (RequestFailedException exception)
        {
            // This is how the SDK reports a send that did not go through: a rejected
            // request, an unreachable service, and also a send the service finished as
            // failed or canceled. It is turned into a result because a caller has to
            // decide what a missing mail means. For the consent record it means stop.
            _logger.LogError(
                exception,
                "Sending email with subject '{Subject}' failed: '{ErrorCode}' (HTTP {Status})",
                subject,
                exception.ErrorCode,
                exception.Status);

            return Result.Fail(new ExceptionalError($"Sending email with subject '{subject}' failed.", exception));
        }

        if (status != EmailSendStatus.Succeeded)
        {
            // Not expected from the SDK version in use, which throws instead. Checked all
            // the same, so that a mail is never reported as sent on anything but a success.
            _logger.LogError("Sending email with subject '{Subject}' failed: '{Status}'", subject, status);

            return Result.Fail($"Sending email with subject '{subject}' ended with status '{status}'.");
        }

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Email sent successfully");
        }

        return Result.Ok();
    }
}