using AdoExport.Api.Configuration;
using AdoExport.Api.Services.Notifications;

using FluentResults;

using Microsoft.Extensions.Options;

namespace AdoExport.Api.Services.UserConsent;

internal sealed class UserConsentService : IUserConsentService
{
    private readonly ISendMailService _sendMailService;

    private readonly IOptions<UserConsentMailConfiguration> _userConsentMailOptions;

    public UserConsentService(
        ISendMailService sendMailService,
        IOptions<UserConsentMailConfiguration> userConsentMailOptions)
    {
        Guard.IsNotNull(sendMailService);
        Guard.IsNotNull(userConsentMailOptions);

        _sendMailService = sendMailService;
        _userConsentMailOptions = userConsentMailOptions;
    }

    /// <inheritdoc />
    public async Task<Result> StoreUserConsentForContactFormAsync(string userIdentifier, DateTime timeOfConsent)
    {
        await _sendMailService.SendUserConsentMailAsync(
            _userConsentMailOptions.Value.Sender,
            _userConsentMailOptions.Value.Receiver,
            new ContactFormUserConsent()
            {
                Identifier = userIdentifier,
                TimeOfConsent = timeOfConsent,
            }).ConfigureAwait(false);

        return Result.Ok();
    }

    /// <inheritdoc />
    public async Task<Result> StoreUserConsentForFeedbackFormAsync(string auditId, string userIdentifier, DateTime timeOfConsent)
    {
        await _sendMailService.SendUserConsentMailAsync(
            _userConsentMailOptions.Value.Sender,
            _userConsentMailOptions.Value.Receiver,
            new FeedbackFormUserConsent()
            {
                AuditId = auditId,
                Identifier = userIdentifier,
                TimeOfConsent = timeOfConsent,
            }).ConfigureAwait(false);

        return Result.Ok();
    }

    /// <inheritdoc />
    public async Task<Result> StoreUserConsentForAuditAsync(string auditId, string userIdentifier, bool consentForAnonymousLogging, DateTime timeOfConsent)
    {
        await _sendMailService.SendUserConsentMailAsync(
            _userConsentMailOptions.Value.Sender,
            _userConsentMailOptions.Value.Receiver,
            new StartAuditFormUserConsent()
            {
                AuditId = auditId,
                Identifier = userIdentifier,
                TimeOfConsent = timeOfConsent,
                ConsentForAnonymousLogging = consentForAnonymousLogging,
            }).ConfigureAwait(false);

        return Result.Ok();
    }
}