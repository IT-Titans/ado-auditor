using FluentResults;

namespace AdoExport.Api.Services.Notifications;

public interface ISendMailService
{
    /// <summary>
    /// Sends an email.
    /// </summary>
    /// <returns>
    /// A successful result once the mail service has accepted the mail for delivery, and a
    /// failed one when it has not. A caller for whom the mail is a record, such as the
    /// record of a user's consent, must not carry on after a failed result.
    /// </returns>
    Task<Result> SendMailAsync(string sender, string receiver, string subject, string body);
}