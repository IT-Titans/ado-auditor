using System.Text.Json;

using AdoExport.Api.Auditing;
using AdoExport.Api.Configuration;
using AdoExport.Api.Controllers.Mappings;
using AdoExport.Api.Controllers.Transformers;
using AdoExport.Api.RateLimiting;
using AdoExport.Api.Reports;
using AdoExport.Api.Reports.DataSources;
using AdoExport.Api.Reports.Models;
using AdoExport.Api.Reports.Templates;
using AdoExport.Api.Repositories;
using AdoExport.Api.Services;
using AdoExport.Api.Services.Caching;
using AdoExport.Api.Services.Notifications;
using AdoExport.Api.Services.UserConsent;
using AdoExport.Auditing;
using AdoExport.Client.AdoApi;
using AdoExport.Common.Services;

using FluentValidation;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace AdoExport.Api;

internal static class HostingExtensions
{
    private const int InvalidConfigurationExitCode = -1;

    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        builder.Services.AddSingleton<IAuditMapper, AuditMapper>();
        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<IDateTimeService, DateTimeService>();

        builder
            .AddJsonSerializerOptions()
            .AddApiClient()
            .AddCaches()
            .AddAuditor()
            .AddFeedback()
            .AddErrorReporting()
            .AddUserConsent()
            .AddPdfReports()
            .AddRateLimiting();

        builder.Services.AddScoped<AuditControllerTransformer>();

        builder.Services.AddDataProtection();

        return builder;
    }

    private static WebApplicationBuilder AddFeedback(this WebApplicationBuilder builder)
    {
        builder
            .Services
            .AddOptionsWithFluentValidation<FeedbackMailConfiguration>(FeedbackMailConfiguration.ConfigurationSectionName)
            .AddOptionsWithFluentValidation<ContactMailConfiguration>(ContactMailConfiguration.ConfigurationSectionName)
            .AddOptionsWithFluentValidation<AzureCommunicationServiceConfiguration>(AzureCommunicationServiceConfiguration.ConfigurationSectionName)
            .AddSingleton<IEmailClient, AzureCommunicationsEmailClient>()
            .AddSingleton<ISendMailService, AzureCommunicationsSendMailService>();

        return builder;
    }

    private static WebApplicationBuilder AddErrorReporting(this WebApplicationBuilder builder)
    {
        builder
            .Services
            .AddOptionsWithFluentValidation<ErrorReportsMailConfiguration>(ErrorReportsMailConfiguration.ConfigurationSectionName);

        return builder;
    }

    private static WebApplicationBuilder AddUserConsent(this WebApplicationBuilder builder)
    {
        builder
            .Services
            .AddOptionsWithFluentValidation<UserConsentMailConfiguration>(UserConsentMailConfiguration.ConfigurationSectionName)
            .AddSingleton<IUserConsentService, UserConsentService>();

        return builder;
    }

    private static WebApplicationBuilder AddApiClient(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAdoApiClient, AdoApiClient>();
        builder.Services.AddScoped<IClientConfigProvider, ClientConfigProvider>();
        builder.Services.AddScoped<IRetrieveAuditDataService, RetrieveAuditDataService>();

        return builder;
    }

    private static WebApplicationBuilder AddAuditor(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAuditor, Auditor>();
        builder.Services.AddOptionsWithFluentValidation<AuditLogsMailConfiguration>(AuditLogsMailConfiguration.ConfigurationSectionName);

        builder.Services.AddScoped<ScopedAuditKeyBearer>();
        builder.Services.AddScoped<ScopedUserCredentialsBearer>();

        builder.Services.AddScoped<IAuditService, AuditService>();

        builder.Services.AddScoped<INotesOnRulesService, NotesOnRulesService>();
        builder.Services.AddScoped<IAuditSummarizerService, AuditSummarizerService>();

        builder.Services.AddScoped<IAuditRepository, AuditRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ISessionTokenRepository, SessionTokenRepository>();
        builder.Services.AddScoped<IRulesRepository, RulesRepository>();

        builder.Services.AddScoped<IUsersFacade, UsersFacade>();
        builder.Services.AddScoped<IPatsFacade, PatsFacade>();

        return builder;
    }

    private static WebApplicationBuilder AddPdfReports(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IPdfReportService, PdfReportService>();
        builder.Services.AddScoped<IAuditReportDataSource, AuditReportDataSource>();

        builder.Services.AddScoped<Func<AuditPdfModel, AuditPdfReportDocument>>(services =>
        {
            return auditData => new AuditPdfReportDocument(
                services.GetRequiredService<IDateTimeService>(),
                auditData);
        });

        return builder;
    }

    private static WebApplicationBuilder AddCaches(this WebApplicationBuilder builder)
    {
        builder
            .Services
            .AddOptionsWithFluentValidation<CacheConfiguration>(CacheConfiguration.ConfigurationSectionName);

        var cacheType = builder
            .Configuration
            .GetRequiredSection(CacheConfiguration.ConfigurationSectionName)
            .GetValue<CacheTypes?>(nameof(CacheConfiguration.CacheType));

        if (cacheType is null)
        {
            Console.WriteLine($"Invalid configuration for '{CacheConfiguration.ConfigurationSectionName}'. Specify a value for '{nameof(CacheConfiguration.CacheType)}'");
            Environment.Exit(InvalidConfigurationExitCode);
            return builder;
        }

        builder.Services.AddSingleton<IAuditCache, AuditCache>();
        builder.Services.AddSingleton<IUserDataCache, UserDataCache>();
        builder.Services.AddSingleton<ICreateAuditDataCache, CreateAuditDataCache>();

        builder.Services.AddSingleton<IInMemoryCachingService, InMemoryCachingService>();

        switch (cacheType)
        {
            case CacheTypes.Memory:
                builder.Services.AddSingleton<ICachingService, InMemoryCachingService>();
                break;

            case CacheTypes.Redis:
                builder
                    .Services
                    .AddOptionsWithFluentValidation<RedisCacheConfiguration>(RedisCacheConfiguration.ConfigurationSectionName);

                builder.Services.AddSingleton<IConnectionMultiplexer>(services =>
                {
                    var redisConfig = services.GetRequiredService<IOptions<RedisCacheConfiguration>>();
                    return ConnectionMultiplexer.Connect(redisConfig.Value.Configuration);
                });
                builder.Services.AddSingleton<ICachingService, RedisCachingService>();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, "Unknown cache type");
        }

        return builder;
    }

    private static WebApplicationBuilder AddJsonSerializerOptions(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(_ => new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        return builder;
    }
}