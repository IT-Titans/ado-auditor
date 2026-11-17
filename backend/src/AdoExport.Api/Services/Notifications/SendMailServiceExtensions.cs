using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentResults;

namespace AdoExport.Api.Services.Notifications;

public static class SendMailServiceExtensions
{
    /// <summary>
    /// Sends a feedback mail.
    /// </summary>
    /// <param name="sendMailService" />
    /// <param name="sender">The senders email.</param>
    /// <param name="receiver">The receivers email.</param>
    /// <param name="feedback">The feedback.</param>
    /// <returns>The operations result.</returns>
    public static Task<Result> SendFeedbackMailAsync(
        this ISendMailService sendMailService,
        string sender,
        string receiver,
        Feedback feedback)
    {
        Guard.IsNotNull(sendMailService);
        Guard.IsNotNullOrWhiteSpace(sender);
        Guard.IsNotNullOrWhiteSpace(receiver);
        Guard.IsNotNull(feedback);

        var body = new StringBuilder();
        body
            .Append("Audit: ").AppendLine(feedback.AuditId)
            .Append("Name: ").AppendLine(feedback.Name)
            .Append("Email: ").AppendLine(feedback.Email)
            .Append("Feedback: ").AppendLine(feedback.Text);

        return sendMailService.SendMailAsync(sender, receiver, EnrichSubject("Feedback"), body.ToString());
    }

    /// <summary>
    /// Sends a contact inquiry mail.
    /// </summary>
    /// <param name="sendMailService" />
    /// <param name="sender">The senders email.</param>
    /// <param name="receiver">The receivers email.</param>
    /// <param name="contactInquiry">The inquiry.</param>
    /// <returns>The operations result.</returns>
    public static Task<Result> SendContactMailAsync(
        this ISendMailService sendMailService,
        string sender,
        string receiver,
        ContactInquiry contactInquiry)
    {
        Guard.IsNotNull(sendMailService);
        Guard.IsNotNullOrWhiteSpace(sender);
        Guard.IsNotNullOrWhiteSpace(receiver);
        Guard.IsNotNull(contactInquiry);

        var body = new StringBuilder();
        body
            .Append("Name: ").AppendLine(contactInquiry.Name)
            .Append("Email: ").AppendLine(contactInquiry.Email)
            .Append("Phone: ").AppendLine(contactInquiry.PhoneNumber)
            .Append("Message: ").AppendLine(contactInquiry.Text);

        return sendMailService.SendMailAsync(sender, receiver, EnrichSubject("Contact Inquiry"), body.ToString());
    }

    /// <summary>
    /// Sends a user consent mail.
    /// </summary>
    /// <param name="sendMailService" />
    /// <param name="sender">The senders email.</param>
    /// <param name="receiver">The receivers email.</param>
    /// <param name="userConsent">The user consent information.</param>
    /// <returns>The operations result.</returns>
    public static Task<Result> SendUserConsentMailAsync<TConsent>(
        this ISendMailService sendMailService,
        string sender,
        string receiver,
        TConsent userConsent)
    where TConsent : UserConsent
    {
        Guard.IsNotNull(sendMailService);
        Guard.IsNotNullOrWhiteSpace(sender);
        Guard.IsNotNullOrWhiteSpace(receiver);
        Guard.IsNotNull(userConsent);

        var body = JsonSerializer.Serialize(userConsent, new JsonSerializerOptions()
        {
            Converters =
            {
                new JsonStringEnumConverter(),
            }
        });

        return sendMailService.SendMailAsync(sender, receiver, EnrichSubject("User Consent"), body);
    }

    /// <summary>
    /// Sends an audit metrics mail.
    /// </summary>
    /// <param name="sendMailService" />
    /// <param name="sender">The senders email.</param>
    /// <param name="receiver">The receivers email.</param>
    /// <param name="auditMetrics">The audit metrics.</param>
    /// <returns>The operations result.</returns>
    public static Task<Result> SendAuditMetricsMailAsync(
        this ISendMailService sendMailService,
        string sender,
        string receiver,
        AuditMetrics auditMetrics)
    {
        Guard.IsNotNull(sendMailService);
        Guard.IsNotNullOrWhiteSpace(sender);
        Guard.IsNotNullOrWhiteSpace(receiver);
        Guard.IsNotNull(auditMetrics);

        var body = JsonSerializer.Serialize(auditMetrics);

        return sendMailService.SendMailAsync(sender, receiver, EnrichSubject("Audit metrics"), body);
    }

    /// <summary>
    /// Sends an error report mail.
    /// </summary>
    /// <param name="sendMailService" />
    /// <param name="sender">The senders email.</param>
    /// <param name="receiver">The receivers email.</param>
    /// <param name="errorReport">The error report.</param>
    /// <returns>The operations result.</returns>
    public static Task<Result> SendErrorReportMailAsync(
        this ISendMailService sendMailService,
        string sender,
        string receiver,
        ErrorReport errorReport)
    {
        Guard.IsNotNull(sendMailService);
        Guard.IsNotNullOrWhiteSpace(sender);
        Guard.IsNotNullOrWhiteSpace(receiver);
        Guard.IsNotNull(errorReport);

        var body = JsonSerializer.Serialize(errorReport);

        return sendMailService.SendMailAsync(sender, receiver, EnrichSubject("Error reports"), body);
    }

    /// <summary>
    /// Enriches the given <paramref name="subject" />. 
    /// </summary>
    /// <param name="subject">The subject to enrich.</param>
    /// <returns>An enriched subject.</returns>
    private static string EnrichSubject(string subject)
    {
        return $"ADO Auditor [{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}] - {subject}";
    }
}