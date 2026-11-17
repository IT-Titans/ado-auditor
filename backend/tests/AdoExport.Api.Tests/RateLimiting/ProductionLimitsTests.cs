using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using AdoExport.Api.Configuration;
using AdoExport.Api.Services.Notifications;

using Azure.Communication.Email;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace AdoExport.Api.Tests.RateLimiting;

/// <summary>
/// The limits an instance really runs with, depending on the branch its image was built
/// from.
/// </summary>
/// <remarks>
/// Production is deployed from main and gets raised limits for as long as all visitors
/// share one budget. Everything else, the dev environment included, must keep the strict
/// defaults. The host here reads the settings files that ship with the application, so
/// these cases fail when someone changes a value in one of them without meaning to.
/// </remarks>
public class ProductionLimitsTests
{
    private static async Task<HttpStatusCode> SendContactRequestAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/contact",
            new { acceptedDataPolicy = true, email = "someone@example.com", message = "Is the audit result cached anywhere?" });

        return response.StatusCode;
    }

    [Test]
    public void AnImageBuiltFromMainRunsWithTheRaisedLimits()
    {
        using var factory = new ShippedSettingsApiFactory(branch: "main");

        var limits = factory.Services.GetRequiredService<IOptions<RateLimitingConfiguration>>().Value;

        limits.AuditPermitLimit.Should().Be(60);
        limits.MailPermitLimit.Should().Be(20);
        limits.GlobalPermitLimit.Should().Be(3000);
    }

    [Test]
    public void AnImageBuiltFromMainKeepsEverythingElseAtItsDefault()
    {
        using var factory = new ShippedSettingsApiFactory(branch: "main");

        var limits = factory.Services.GetRequiredService<IOptions<RateLimitingConfiguration>>().Value;

        limits.Enabled.Should().BeTrue();
        limits.Window.Should().Be(TimeSpan.FromMinutes(10));
        limits.QueueLimit.Should().Be(0);
        limits.TrustedProxyKey.Should().BeNullOrEmpty();
    }

    [TestCase(null, Description = "a local build")]
    [TestCase("dev", Description = "the dev environment")]
    [TestCase("merge", Description = "a pull request build")]
    public void EveryOtherImageKeepsTheStrictDefaults(string? branch)
    {
        using var factory = new ShippedSettingsApiFactory(branch);

        var limits = factory.Services.GetRequiredService<IOptions<RateLimitingConfiguration>>().Value;

        limits.AuditPermitLimit.Should().Be(5);
        limits.MailPermitLimit.Should().Be(3);
        limits.GlobalPermitLimit.Should().Be(300);
    }

    [Test]
    public async Task AnImageBuiltFromMainAcceptsTwentyMailsAndRejectsTheNext()
    {
        // The options alone would not prove that the limiter uses them.
        using var factory = new ShippedSettingsApiFactory(branch: "main");
        using var client = factory.CreateClient();

        for (var attempt = 1; attempt <= 20; attempt++)
        {
            (await SendContactRequestAsync(client)).Should().Be(HttpStatusCode.OK, $"request {attempt} is within the production limit");
        }

        (await SendContactRequestAsync(client)).Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Test]
    public async Task AnImageBuiltFromDevRejectsTheFourthMail()
    {
        using var factory = new ShippedSettingsApiFactory(branch: "dev");
        using var client = factory.CreateClient();

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            (await SendContactRequestAsync(client)).Should().Be(HttpStatusCode.OK);
        }

        (await SendContactRequestAsync(client)).Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Test]
    public void TheProductionFileChangesTheThreeLimitsAndNothingElse()
    {
        // The file is loaded into production unseen by anyone at deployment time. A
        // setting slipped into it would go live the same way.
        var path = Path.Combine(ShippedSettingsApiFactory.ApplicationDirectory, "appsettings.branch.main.json");
        using var document = JsonDocument.Parse(
            File.ReadAllText(path),
            new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });

        document.RootElement.EnumerateObject().Select(property => property.Name)
            .Should().Equal("RateLimiting");
        document.RootElement.GetProperty("RateLimiting").EnumerateObject().Select(property => property.Name)
            .Should().BeEquivalentTo("AuditPermitLimit", "MailPermitLimit", "GlobalPermitLimit");
    }

    [Test]
    public void TheProductionFileSaysThatItIsTemporary()
    {
        var path = Path.Combine(ShippedSettingsApiFactory.ApplicationDirectory, "appsettings.branch.main.json");

        File.ReadAllText(path).Should().Contain("TEMPORARY").And.Contain("RATELIMITING__TRUSTEDPROXYKEY");
    }

    /// <summary>
    /// Hosts the application with the settings files it ships with. Nothing about rate
    /// limiting is configured here, which is the point: the values come from the files.
    /// </summary>
    private sealed class ShippedSettingsApiFactory : WebApplicationFactory<Program>
    {
        private readonly string? _branch;

        public ShippedSettingsApiFactory(string? branch)
        {
            _branch = branch;
        }

        /// <summary>The directory the application's settings files are copied to on build.</summary>
        public static string ApplicationDirectory => Path.GetDirectoryName(typeof(Program).Assembly.Location)!;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");

            // The settings files are read from where the build put them, as they are in
            // the image, and not from the source directory.
            builder.UseContentRoot(ApplicationDirectory);

            if (_branch is not null)
            {
                // What ADOAUDITOR__BUILDBRANCH is in the image. It has to be known before
                // the application configures itself, hence a host setting.
                builder.UseSetting(BranchSettings.BranchKey, _branch);
            }

            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Feedback:Receiver"] = "feedback@example.com",
                    ["Contact:Receiver"] = "contact@example.com",
                    ["UserConsent:Receiver"] = "consent@example.com",
                    ["AuditLogs:Receiver"] = "auditlogs@example.com",
                    ["ErrorReports:Receiver"] = "errors@example.com",
                    ["AzureCommunicationService:ConnectionString"] = "endpoint=https://example.communication.azure.com/;accesskey=dGVzdA==",
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IEmailClient>();
                services.AddSingleton<IEmailClient, AcceptingEmailClient>();
            });
        }

        private sealed class AcceptingEmailClient : IEmailClient
        {
            public Task<EmailSendStatus> SendAsync(string sender, string receiver, string subject, string plainTextBody)
                => Task.FromResult(EmailSendStatus.Succeeded);
        }
    }
}
