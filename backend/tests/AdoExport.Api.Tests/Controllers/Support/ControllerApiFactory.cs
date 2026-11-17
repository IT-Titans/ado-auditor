using System.Collections.Concurrent;

using AdoExport.Api.Reports;
using AdoExport.Api.Services;
using AdoExport.Api.Services.Caching;
using AdoExport.Api.Services.Notifications;
using AdoExport.Api.Services.UserConsent;
using AdoExport.Auditing;
using AdoExport.Client.AdoApi.Errors;
using AdoExport.Common.Errors;

using FluentResults;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AdoExport.Api.Tests.Controllers.Support;

/// <summary>
/// Hosts the real pipeline in memory for the controller tests.
/// </summary>
/// <remarks>
/// Everything that would leave the process is replaced: mail, consent storage, the Azure
/// DevOps backed audit creation and PDF rendering. The caches, repositories, facades and
/// mappers are the real ones, so a GET returns what an audit seeded into the cache really
/// produces.
/// <para>
/// The stubs are steered by their input (organization name, e-mail address, audit id) rather
/// than by mutable state, so tests can run in parallel against one shared factory.
/// </para>
/// </remarks>
public sealed class ControllerApiFactory : WebApplicationFactory<Program>
{
    public const string OrganizationOk = "ok-org";

    public const string OrganizationNotFound = "missing-org";

    public const string OrganizationInsufficientRights = "no-rights-org";

    public const string OrganizationInvalidPat = "invalid-pat-org";

    public const string OrganizationUnexpectedFailure = "broken-org";

    /// <summary>E-mail address for which storing consent fails.</summary>
    public const string EmailConsentFails = "consent-fails@example.com";

    /// <summary>Text which makes the mail carrying it fail to send.</summary>
    public const string TextMailFails = "this-mail-cannot-be-sent";

    private static int _clientCounter;

    public RecordingMailService Mail { get; } = new();

    public StubPdfReportService Pdf { get; } = new();

    /// <summary>
    /// Creates a client that identifies as a unique remote address, so the rate limiter
    /// treats every test as its own caller.
    /// </summary>
    public HttpClient CreateUniqueClient()
    {
        var client = CreateClient();
        var number = Interlocked.Increment(ref _clientCounter);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", $"198.51.{(number >> 8) & 0xFF}.{number & 0xFF}");

        return client;
    }

    /// <summary>
    /// Puts an audit (and the raw data behind it) into the caches and returns the audit key.
    /// </summary>
    public async Task<Guid> SeedAuditAsync(TestAuditData? data = null)
    {
        data ??= TestAuditData.Create();

        using var scope = Services.CreateScope();
        var key = data.Audit.AuditId.ToString();

        await scope.ServiceProvider.GetRequiredService<IAuditCache>().SetAsync(key, data.Audit);
        await scope.ServiceProvider.GetRequiredService<ICreateAuditDataCache>().SetAsync(key, data.CreateAuditData);

        return data.Audit.AuditId;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Feedback:Sender"] = "DoNotReply@example.com",
                ["Feedback:Receiver"] = "feedback@example.com",
                ["Contact:Sender"] = "DoNotReply@example.com",
                ["Contact:Receiver"] = "contact@example.com",
                ["UserConsent:Sender"] = "DoNotReply@example.com",
                ["UserConsent:Receiver"] = "consent@example.com",
                ["AuditLogs:Sender"] = "DoNotReply@example.com",
                ["AuditLogs:Receiver"] = "auditlogs@example.com",
                ["ErrorReports:Sender"] = "DoNotReply@example.com",
                ["ErrorReports:Receiver"] = "errors@example.com",
                ["AzureCommunicationService:ConnectionString"] = "endpoint=https://example.communication.azure.com/;accesskey=dGVzdA==",
                ["Caches:CacheType"] = "Memory",
                ["Caches:CacheExpiry"] = "00:30:00",

                // Rate limiting stays on, with limits no single test comes close to.
                ["RateLimiting:Enabled"] = "true",
                ["RateLimiting:Window"] = "00:10:00",
                ["RateLimiting:MailPermitLimit"] = "1000",
                ["RateLimiting:AuditPermitLimit"] = "1000",
                ["RateLimiting:GlobalPermitLimit"] = "100000",
                ["RateLimiting:QueueLimit"] = "0",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ISendMailService>();
            services.AddSingleton<ISendMailService>(Mail);

            services.RemoveAll<IUserConsentService>();
            services.AddSingleton<IUserConsentService, StubUserConsentService>();

            services.RemoveAll<IAuditService>();
            services.AddScoped<IAuditService, StubAuditService>();

            services.RemoveAll<IPdfReportService>();
            services.AddSingleton<IPdfReportService>(Pdf);
        });
    }

    public sealed class RecordingMailService : ISendMailService
    {
        private readonly ConcurrentBag<(string Subject, string Body)> _mails = new();

        public IReadOnlyCollection<(string Subject, string Body)> Mails => _mails;

        public Task<Result> SendMailAsync(string sender, string receiver, string subject, string body)
        {
            if (body.Contains(TextMailFails, StringComparison.Ordinal))
            {
                return Task.FromResult(Result.Fail("mail service unavailable"));
            }

            _mails.Add((subject, body));
            return Task.FromResult(Result.Ok());
        }
    }

    public sealed class StubPdfReportService : IPdfReportService
    {
        private readonly ConcurrentDictionary<Guid, Result<PdfFileInfo>> _results = new();

        public void Register(Guid auditId, Result<PdfFileInfo> result) => _results[auditId] = result;

        public Task<Result<PdfFileInfo>> GetAuditReportPdfFileAsync(Guid auditId, CancellationToken cancellationToken)
            => Task.FromResult(_results.TryGetValue(auditId, out var result)
                ? result
                : Result.Fail<PdfFileInfo>(new Api.Errors.AuditExpiredError(auditId.ToString())));
    }

    private sealed class StubUserConsentService : IUserConsentService
    {
        public Task<Result> StoreUserConsentForContactFormAsync(string userIdentifier, DateTime timeOfConsent)
            => Task.FromResult(Outcome(userIdentifier));

        public Task<Result> StoreUserConsentForFeedbackFormAsync(string auditId, string userIdentifier, DateTime timeOfConsent)
            => Task.FromResult(Outcome(userIdentifier));

        public Task<Result> StoreUserConsentForAuditAsync(string auditId, string userIdentifier, bool consentForAnonymousLogging, DateTime timeOfConsent)
            => Task.FromResult(Outcome(userIdentifier));

        private static Result Outcome(string userIdentifier)
            => userIdentifier == EmailConsentFails ? Result.Fail("consent store unavailable") : Result.Ok();
    }

    /// <summary>
    /// Reads the organization the controller put into the user data cache for the duration
    /// of the call, exactly where the real service would look for it, and answers by name.
    /// </summary>
    private sealed class StubAuditService(IUserDataCache userDataCache) : IAuditService
    {
        public async Task<Result<Audit>> CreateAuditAsync(Guid auditId, CancellationToken cancellationToken)
        {
            var credentials = await userDataCache.GetAsync(auditId.ToString());

            return credentials?.Organization switch
            {
                OrganizationOk => Result.Ok(TestAuditData.Create(auditId).Audit),
                OrganizationNotFound => Result.Fail(new OrganizationNotFoundError()),
                OrganizationInsufficientRights => Result.Fail(new InsufficientRightsError()),
                OrganizationInvalidPat => Result.Fail(new InvalidPatError()),
                _ => Result.Fail("unexpected failure"),
            };
        }
    }
}
