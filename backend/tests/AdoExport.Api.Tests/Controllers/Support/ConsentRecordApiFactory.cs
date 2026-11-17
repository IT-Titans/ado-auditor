using System.Collections.Concurrent;

using AdoExport.Api.Services;
using AdoExport.Api.Services.Notifications;
using AdoExport.Auditing;

using Azure;
using Azure.Communication.Email;

using FluentResults;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AdoExport.Api.Tests.Controllers.Support;

/// <summary>
/// Hosts the real pipeline with the real consent and mail services, and replaces only the
/// last step: the client that would talk to Azure Communication Services.
/// </summary>
/// <remarks>
/// <see cref="ControllerApiFactory" /> swaps the consent service for a stub, which shows
/// what a controller does with a failed result but not that a failed send produces one.
/// Here the whole chain runs, so a test can say "the consent mail did not go out" and
/// watch what reaches the user.
/// </remarks>
public sealed class ConsentRecordApiFactory : WebApplicationFactory<Program>
{
    public const string ConsentSubject = "User Consent";

    public const string MetricsSubject = "Audit metrics";

    public FakeEmailClient EmailClient { get; } = new();

    public CountingAuditService AuditService { get; } = new();

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
                ["RateLimiting:Enabled"] = "false",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailClient>();
            services.AddSingleton<IEmailClient>(EmailClient);

            services.RemoveAll<IAuditService>();
            services.AddSingleton<IAuditService>(AuditService);
        });
    }

    /// <summary>
    /// Stands in for Azure Communication Services. Every mail succeeds unless a test says
    /// what should happen to mails with a given subject.
    /// </summary>
    public sealed class FakeEmailClient : IEmailClient
    {
        private readonly ConcurrentQueue<string> _sentSubjects = new();

        private readonly ConcurrentDictionary<string, Func<EmailSendStatus>> _outcomes = new();

        /// <summary>The subjects of the mails that went out, in order.</summary>
        public IReadOnlyCollection<string> SentSubjects => _sentSubjects;

        /// <summary>Makes mails whose subject contains <paramref name="subject" /> end with <paramref name="status" />.</summary>
        public void EndWith(string subject, EmailSendStatus status) => _outcomes[subject] = () => status;

        /// <summary>Makes mails whose subject contains <paramref name="subject" /> fail the way the SDK reports it.</summary>
        public void Throw(string subject)
            => _outcomes[subject] = () => throw new RequestFailedException(500, "The mail could not be delivered.", "EmailDropped", innerException: null);

        public Task<EmailSendStatus> SendAsync(string sender, string receiver, string subject, string plainTextBody)
        {
            foreach (var (marker, outcome) in _outcomes)
            {
                if (subject.Contains(marker, StringComparison.Ordinal))
                {
                    var status = outcome();
                    if (status == EmailSendStatus.Succeeded)
                    {
                        _sentSubjects.Enqueue(subject);
                    }

                    return Task.FromResult(status);
                }
            }

            _sentSubjects.Enqueue(subject);

            return Task.FromResult(EmailSendStatus.Succeeded);
        }
    }

    /// <summary>
    /// Answers every audit with success and counts how often it was asked, which is the
    /// evidence that an audit did or did not start.
    /// </summary>
    public sealed class CountingAuditService : IAuditService
    {
        private int _started;

        public int Started => Volatile.Read(ref _started);

        public Task<Result<Audit>> CreateAuditAsync(Guid auditId, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _started);

            return Task.FromResult(Result.Ok(TestAuditData.Create(auditId).Audit));
        }
    }
}
