using Azure;
using Azure.Communication.Email;

namespace AdoExport.Api.Services.Notifications;

/// <summary>
/// The one call into Azure Communication Services that sending a mail needs.
/// </summary>
/// <remarks>
/// Exists so that what happens when a send does not succeed can be tested. The SDK client
/// can only be driven by talking to the service, and whether a mail went out decides
/// more here than delivery: the record of a user's consent is a mail as well.
/// </remarks>
public interface IEmailClient
{
    /// <summary>
    /// Hands a plain text mail to the service and waits until the service has finished
    /// processing it.
    /// </summary>
    /// <returns>The final status the service reports for the mail.</returns>
    /// <exception cref="RequestFailedException">
    /// The service rejected the request, could not be reached, or finished the send as
    /// failed or canceled.
    /// </exception>
    Task<EmailSendStatus> SendAsync(string sender, string receiver, string subject, string plainTextBody);
}
