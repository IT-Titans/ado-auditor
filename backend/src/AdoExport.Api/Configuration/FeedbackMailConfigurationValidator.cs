using FluentValidation;

using JetBrains.Annotations;

namespace AdoExport.Api.Configuration;

[UsedImplicitly(Reason = "Registered to the DI automatically")]
public class FeedbackMailConfigurationValidator : AbstractValidator<FeedbackMailConfiguration>
{
    public FeedbackMailConfigurationValidator()
    {
        RuleFor(x => x.Sender)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Receiver)
            .NotEmpty()
            .EmailAddress();
    }
}