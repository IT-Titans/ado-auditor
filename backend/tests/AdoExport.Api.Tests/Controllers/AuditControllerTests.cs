using System.Net;
using System.Net.Http.Json;

using AdoExport.Api.Configuration;
using AdoExport.Api.Controllers;
using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Services.Notifications;
using AdoExport.Api.Tests.Controllers.Support;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AdoExport.Api.Tests.Controllers;

public class AuditControllerTests : ControllerTestBase
{
    private static object ValidRequest(
        string organization = ControllerApiFactory.OrganizationOk,
        string email = "someone@example.com",
        bool? acceptedDataPolicy = true,
        bool? acceptedAnonymousAuditLogging = false) => new
        {
            organization,
            pat = "a-personal-access-token",
            email,
            acceptedDataPolicy,
            acceptedAnonymousAuditLogging,
        };

    // --- POST /api/audit ---

    [Test]
    public async Task StartAudit_WithValidRequest_ReturnsTheAuditId()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.PostAsJsonAsync("/api/audit", ValidRequest(acceptedAnonymousAuditLogging: true));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auditId = await response.Content.ReadFromJsonAsync<Guid>();
        auditId.Should().NotBeEmpty();

        // A finished audit reports its timings, and the organization because consent was given.
        Factory.Mail.Mails
            .Should()
            .ContainSingle(mail => mail.Body.Contains(auditId.ToString()))
            .Which.Body.Should().Contain("organizationName").And.Contain("apisInUseFor");
    }

    [Test]
    public async Task StartAudit_WithoutConsentToAnonymousLogging_KeepsTheOrganizationOutOfTheMetricsMail()
    {
        using var client = Factory.CreateUniqueClient();
        var organization = $"unlisted-org-{Guid.NewGuid():N}";

        // Any organization other than the known failures ends in a failed audit, but the
        // metrics mail is sent either way, so it is what is observed here.
        var response = await client.PostAsJsonAsync("/api/audit", ValidRequest(organization, acceptedAnonymousAuditLogging: false));

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        Factory.Mail.Mails
            .Where(mail => mail.Body.Contains(organization, StringComparison.Ordinal))
            .Should()
            .BeEmpty("the organization name may only be logged with explicit consent");
    }

    [Test]
    public async Task StartAudit_WithConsentToAnonymousLogging_PutsTheOrganizationIntoTheMetricsMail()
    {
        using var client = Factory.CreateUniqueClient();
        var organization = $"unlisted-org-{Guid.NewGuid():N}";

        var response = await client.PostAsJsonAsync("/api/audit", ValidRequest(organization, acceptedAnonymousAuditLogging: true));

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        Factory.Mail.Mails
            .Should()
            .ContainSingle(mail => mail.Body.Contains(organization, StringComparison.Ordinal))
            .Which.Subject.Should().Contain("Audit metrics");
    }

    [Test]
    public async Task StartAudit_WithoutAcceptedDataPolicy_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.PostAsJsonAsync("/api/audit", ValidRequest(acceptedDataPolicy: false));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Consent for privacy policy is required");
    }

    [Test]
    public async Task StartAudit_WithNullDataPolicy_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.PostAsJsonAsync("/api/audit", ValidRequest(acceptedDataPolicy: null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [TestCase("")]
    [TestCase("   ")]
    public async Task StartAudit_WithBlankOrganization_ReturnsBadRequest(string organization)
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.PostAsJsonAsync("/api/audit", ValidRequest(organization));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task StartAudit_WithInvalidEmail_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.PostAsJsonAsync("/api/audit", ValidRequest(email: "not-an-email"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task StartAudit_WhenConsentCannotBeStored_ReturnsServerErrorAndDoesNotStartTheAudit()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.PostAsJsonAsync("/api/audit", ValidRequest(email: ControllerApiFactory.EmailConsentFails));

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        Factory.Mail.Mails.Should().NotContain(mail => mail.Body.Contains(ControllerApiFactory.EmailConsentFails));
    }

    [Test]
    public async Task StartAudit_WhenOrganizationIsNotFound_ReturnsUnauthorized()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.PostAsJsonAsync("/api/audit", ValidRequest(ControllerApiFactory.OrganizationNotFound));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Organization not found");
    }

    [Test]
    public async Task StartAudit_WhenPatLacksRights_ReturnsUnauthorized()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.PostAsJsonAsync("/api/audit", ValidRequest(ControllerApiFactory.OrganizationInsufficientRights));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await response.Content.ReadAsStringAsync()).Should().Contain("PAT scopes are invalid");
    }

    [Test]
    public async Task StartAudit_WhenPatIsInvalid_ReturnsUnauthorized()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.PostAsJsonAsync("/api/audit", ValidRequest(ControllerApiFactory.OrganizationInvalidPat));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await response.Content.ReadAsStringAsync()).Should().Contain("PAT is invalid");
    }

    [Test]
    public async Task StartAudit_WhenAuditFailsUnexpectedly_ReturnsServerError()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.PostAsJsonAsync("/api/audit", ValidRequest(ControllerApiFactory.OrganizationUnexpectedFailure));

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    // The model validation attributes reject blank values before the action runs, so these
    // guards can only be reached by calling the action directly. They stay as a second line
    // of defence if the validation attributes are ever loosened.
    [Test]
    public async Task StartAudit_WhenCalledDirectlyWithBlankOrganization_ReturnsBadRequest()
    {
        var result = await StartAuditDirectlyAsync(organization: " ");

        AssertBadRequest(result, "Organization is required");
    }

    [Test]
    public async Task StartAudit_WhenCalledDirectlyWithBlankPat_ReturnsBadRequest()
    {
        var result = await StartAuditDirectlyAsync(pat: " ");

        AssertBadRequest(result, "PAT is required");
    }

    [Test]
    public async Task StartAudit_WhenCalledDirectlyWithBlankEmail_ReturnsBadRequest()
    {
        var result = await StartAuditDirectlyAsync(email: " ");

        AssertBadRequest(result, "Email is required");
    }

    private static void AssertBadRequest(IActionResult result, string message)
    {
        result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be(message);
    }

    private static Task<IActionResult> StartAuditDirectlyAsync(
        string organization = "org",
        string pat = "pat",
        string email = "someone@example.com")
    {
        var controller = new AuditController(
            Mock.Of<ILogger<AuditController>>(),
            Options.Create(new AuditLogsMailConfiguration() { Sender = "from@example.com", Receiver = "to@example.com" }),
            Mock.Of<ISendMailService>());

        var model = new StartAuditModel()
        {
            Organization = organization,
            Pat = pat,
            Email = email,
            AcceptedDataPolicy = true,
            AcceptedAnonymousAuditLogging = false,
        };

        // The guards return before any of the services is touched.
        return controller.StartAudit(model, CancellationToken.None, null!, null!, null!, null!, null!);
    }

    // --- GET /api/audit/{auditId} ---

    [Test]
    public async Task GetAuditSummary_ForCachedAudit_ReturnsTheSummary()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/audit/{auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.GetProperty("criticalCount").GetInt32().Should().BeGreaterThan(0);
        json.GetProperty("summaries").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GetAuditSummary_ForExpiredAudit_ReturnsGone()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/audit/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Audit not found or expired");
    }

    [Test]
    public async Task GetAuditSummary_WithEmptyAuditId_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/audit/{Guid.Empty}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Audit-Id is required");
    }

    [Test]
    public async Task GetAuditSummary_WithMalformedAuditId_IsNotRouted()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync("/api/audit/not-a-guid");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
