using FluentValidation;

using JetBrains.Annotations;

namespace AdoExport.Api.Configuration;

[UsedImplicitly(Reason = "Registered to the DI automatically")]
public class RedisCacheConfigurationValidator : AbstractValidator<RedisCacheConfiguration>
{
    public RedisCacheConfigurationValidator()
    {
        RuleFor(x => x.Configuration)
            .NotEmpty();
    }
}