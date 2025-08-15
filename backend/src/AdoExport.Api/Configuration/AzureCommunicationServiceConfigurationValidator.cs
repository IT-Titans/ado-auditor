using FluentValidation;

using JetBrains.Annotations;

namespace AdoExport.Api.Configuration;

[UsedImplicitly(Reason = "Registered to the DI automatically")]
public class AzureCommunicationServiceConfigurationValidator : AbstractValidator<AzureCommunicationServiceConfiguration>
{
    public AzureCommunicationServiceConfigurationValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty();
    }
}