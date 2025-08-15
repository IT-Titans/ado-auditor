using Microsoft.Extensions.Options;

namespace AdoExport.Api.Configuration;
public static class OptionsBuilderExtensions
{
    /// <summary>
    /// https://www.milanjovanovic.tech/blog/options-pattern-validation-in-aspnetcore-with-fluentvalidation
    /// </summary>
    public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(
        this OptionsBuilder<TOptions> builder)
        where TOptions : class
    {
        builder.Services.AddSingleton<IValidateOptions<TOptions>>(
            serviceProvider => new FluentValidateOptions<TOptions>(
                serviceProvider,
                builder.Name));

        return builder;
    }
}