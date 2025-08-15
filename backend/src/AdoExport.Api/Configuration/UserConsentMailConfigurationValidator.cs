using FluentValidation;

using JetBrains.Annotations;

namespace AdoExport.Api.Configuration;

[UsedImplicitly(Reason = "Registered to the DI automatically")]
public class UserConsentMailConfigurationValidator : AbstractValidator<UserConsentMailConfiguration>
{
    public UserConsentMailConfigurationValidator()
    {
        RuleFor(x => x.Sender)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Receiver)
            .NotEmpty()
            .EmailAddress();
    }
}