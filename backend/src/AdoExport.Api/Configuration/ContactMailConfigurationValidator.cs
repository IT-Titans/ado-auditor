using FluentValidation;

using JetBrains.Annotations;

namespace AdoExport.Api.Configuration;

[UsedImplicitly(Reason = "Registered to the DI automatically")]
public class ContactMailConfigurationValidator : AbstractValidator<ContactMailConfiguration>
{
    public ContactMailConfigurationValidator()
    {
        RuleFor(x => x.Sender)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Receiver)
            .NotEmpty()
            .EmailAddress();
    }
}