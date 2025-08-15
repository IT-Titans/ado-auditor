using FluentValidation;

using JetBrains.Annotations;

namespace AdoExport.Api.Configuration;

[UsedImplicitly(Reason = "Registered to the DI automatically")]
public class CacheConfigurationValidator : AbstractValidator<CacheConfiguration>
{
    public CacheConfigurationValidator()
    {
        RuleFor(x => x.CacheExpiry)
            .NotEmpty()
            .GreaterThan(TimeSpan.FromMinutes(1));
    }
}