using System.Text.Json.Serialization;

namespace AdoExport.Api.Services.UserConsent;

/// <summary>
/// Defines various possible origins of user consent in this application.
/// </summary>
public enum ConsentOrigins
{
    /// <summary>
    /// For consent given in the feedback form. 
    /// </summary>
    [JsonStringEnumMemberName("feedbackForm")]
    FeedbackForm = 1,

    /// <summary>
    /// For consent given in the contact form.
    /// </summary>
    [JsonStringEnumMemberName("contactForm")]
    ContactForm = 2,

    /// <summary>
    /// For consent given when starting an audit.
    /// </summary>
    [JsonStringEnumMemberName("startAuditForm")]
    StartAuditForm = 3,
}