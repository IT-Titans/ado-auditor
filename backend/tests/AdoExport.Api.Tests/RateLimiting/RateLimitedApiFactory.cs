using AdoExport.Api.Services.Notifications;
using AdoExport.Api.Services.UserConsent;

using FluentResults;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AdoExport.Api.Tests.RateLimiting;

/// <summary>
/// Hosts the real application pipeline in memory, so rate limiting can be observed the
/// way a caller sees it instead of being asserted against configuration.
/// </summary>
/// <remarks>
/// Everything that would reach outside the process is replaced: no mail is sent and no
/// consent is recorded. What is under test is the middleware pipeline.
/// </remarks>
public sealed class RateLimitedApiFactory : WebApplicationFactory<Program>
{
    private readonly int _mailPermitLimit;

    private readonly string? _trustedProxyKey;

    /// <param name="mailPermitLimit">How many mail requests one client may send per window.</param>
    /// <param name="trustedProxyKey">
    /// The shared secret of the frontend server, or <c>null</c> to leave it unconfigured the
    /// way a deployment without the setting would be.
    /// </param>
    public RateLimitedApiFactory(int mailPermitLimit, string? trustedProxyKey = null)
    {
        _mailPermitLimit = mailPermitLimit;
        _trustedProxyKey = trustedProxyKey;
    }

    /// <summary>
    /// The stub standing in for the mail service, which counts what it was asked to send.
    /// </summary>
    public RecordingSendMailService SentMail { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Development would map the OpenAPI endpoints, which these tests do not need.
        builder.UseEnvironment("Production");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Options are validated on start, so every required section has to be
                // present even though these tests never touch most of them.
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

                ["RateLimiting:Enabled"] = "true",
                ["RateLimiting:Window"] = "00:10:00",
                ["RateLimiting:MailPermitLimit"] = _mailPermitLimit.ToString(),
                ["RateLimiting:AuditPermitLimit"] = "5",
                ["RateLimiting:GlobalPermitLimit"] = "300",
                ["RateLimiting:QueueLimit"] = "0",
                ["RateLimiting:TrustedProxyKey"] = _trustedProxyKey,
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ISendMailService>();
            services.AddSingleton<ISendMailService>(SentMail);

            services.RemoveAll<IUserConsentService>();
            services.AddSingleton<IUserConsentService, AlwaysSucceedingUserConsentService>();
        });
    }

    /// <summary>
    /// Stands in for the mail service and counts what it was asked to send. The typed
    /// helpers such as SendContactMailAsync are extension methods, so this single member
    /// covers all of them.
    /// </summary>
    public sealed class RecordingSendMailService : ISendMailService
    {
        private int _sendCount;

        public int SendCount => Volatile.Read(ref _sendCount);

        public Task<Result> SendMailAsync(string sender, string receiver, string subject, string body)
        {
            Interlocked.Increment(ref _sendCount);
            return Task.FromResult(Result.Ok());
        }
    }

    private sealed class AlwaysSucceedingUserConsentService : IUserConsentService
    {
        public Task<Result> StoreUserConsentForContactFormAsync(string userIdentifier, DateTime timeOfConsent)
            => Task.FromResult(Result.Ok());

        public Task<Result> StoreUserConsentForFeedbackFormAsync(string auditId, string userIdentifier, DateTime timeOfConsent)
            => Task.FromResult(Result.Ok());

        public Task<Result> StoreUserConsentForAuditAsync(string auditId, string userIdentifier, bool consentForAnonymousLogging, DateTime timeOfConsent)
            => Task.FromResult(Result.Ok());
    }
}
