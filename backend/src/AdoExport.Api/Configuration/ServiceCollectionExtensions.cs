namespace AdoExport.Api.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// https://www.milanjovanovic.tech/blog/options-pattern-validation-in-aspnetcore-with-fluentvalidation
    /// </summary>
    public static IServiceCollection AddOptionsWithFluentValidation<TOptions>(
        this IServiceCollection services,
        string configurationSection)
        where TOptions : class
    {
        services.AddOptions<TOptions>()
            .BindConfiguration(configurationSection)
            .ValidateFluentValidation() // Configure FluentValidation validation
            .ValidateOnStart(); // Validate options on application start

        return services;
    }
}