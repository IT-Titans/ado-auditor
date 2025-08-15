using FluentResults;

namespace AdoExport.Api.Services.UserConsent;

public interface IUserConsentService
{
    /// <summary>
    /// Stores consent information for the contact form.<br />
    /// When we are to process certain data, we need to store explicit user consent.
    /// </summary>
    /// <param name="userIdentifier">Helps to "identify" the user. Typically, the email address.</param>
    /// <param name="timeOfConsent">The UTC timestamp when consent was given by the user.</param>
    /// <returns>A result indicating success.</returns>
    Task<Result> StoreUserConsentForContactFormAsync(string userIdentifier, DateTime timeOfConsent);

    /// <summary>
    /// Stores consent information for the feedback form.<br />
    /// When we are to process certain data, we need to store explicit user consent.
    /// </summary>
    /// <param name="auditId">Relates the users consent to a specific audit.</param>
    /// <param name="userIdentifier">Helps to "identify" the user. Typically, the email address.</param>
    /// <param name="timeOfConsent">The UTC timestamp when consent was given by the user.</param>
    /// <returns>A result indicating success.</returns>
    Task<Result> StoreUserConsentForFeedbackFormAsync(string auditId, string userIdentifier, DateTime timeOfConsent);

    /// <summary>
    /// Stores consent information related to an audit.<br />
    /// When we are to process certain data, we need to store explicit user consent.
    /// </summary>
    /// <param name="auditId">Relates the users consent to a specific audit.</param>
    /// <param name="userIdentifier">Helps to "identify" the user. Typically, the email address.</param>
    /// <param name="timeOfConsent">The UTC timestamp when consent was given by the user.</param>
    /// <param name="consentForAnonymousLogging">Whether the user gave explicit consent for anonymous audit logging.</param>
    /// <returns>A result indicating success.</returns>
    Task<Result> StoreUserConsentForAuditAsync(string auditId, string userIdentifier, bool consentForAnonymousLogging, DateTime timeOfConsent);
}