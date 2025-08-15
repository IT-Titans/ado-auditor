namespace AdoExport.Api.Services.Notifications;

public interface ISendMailService
{
    /// <summary>
    /// Sends an email.
    /// </summary>
    Task SendMailAsync(string sender, string receiver, string subject, string body);
}