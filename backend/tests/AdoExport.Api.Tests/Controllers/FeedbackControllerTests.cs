using System.Net;
using System.Net.Http.Json;

using AdoExport.Api.Tests.Controllers.Support;

namespace AdoExport.Api.Tests.Controllers;

public class FeedbackControllerTests : ControllerTestBase
{
    private static object ValidFeedback(
        string feedback,
        string? email = "someone@example.com",
        bool acceptedDataPolicy = true) => new
        {
            acceptedDataPolicy,
            name = "Someone",
            email,
            feedback,
        };

    private static string NewText() => $"feedback-{Guid.NewGuid():N}";

    [Test]
    public async Task SubmitFeedback_ForCachedAudit_SendsTheFeedbackMail()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();
        var text = NewText();

        var response = await client.PostAsJsonAsync($"/api/feedback?auditId={auditId}", ValidFeedback(text));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var mail = Factory.Mail.Mails.Should().ContainSingle(m => m.Body.Contains(text)).Subject;
        mail.Subject.Should().Contain("Feedback");
        mail.Body.Should().Contain(auditId.ToString());
    }

    [Test]
    public async Task SubmitFeedback_ForExpiredAudit_StillAcceptsTheFeedback()
    {
        // Deliberate behaviour: a user whose audit timed out should not lose their feedback.
        using var client = Factory.CreateUniqueClient();
        var text = NewText();

        var response = await client.PostAsJsonAsync($"/api/feedback?auditId={Guid.NewGuid()}", ValidFeedback(text));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        Factory.Mail.Mails.Should().ContainSingle(m => m.Body.Contains(text));
    }

    [Test]
    public async Task SubmitFeedback_WithoutEmail_IsAccepted()
    {
        using var client = Factory.CreateUniqueClient();
        var text = NewText();

        var response = await client.PostAsJsonAsync($"/api/feedback?auditId={Guid.NewGuid()}", ValidFeedback(text, email: null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        Factory.Mail.Mails.Should().ContainSingle(m => m.Body.Contains(text));
    }

    [Test]
    public async Task SubmitFeedback_WithoutAuditId_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();
        var text = NewText();

        var response = await client.PostAsJsonAsync("/api/feedback", ValidFeedback(text));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Audit-Id is required");
        Factory.Mail.Mails.Should().NotContain(m => m.Body.Contains(text));
    }

    [Test]
    public async Task SubmitFeedback_WithoutAcceptedDataPolicy_ReturnsBadRequestAndSendsNothing()
    {
        using var client = Factory.CreateUniqueClient();
        var text = NewText();

        var response = await client.PostAsJsonAsync(
            $"/api/feedback?auditId={Guid.NewGuid()}",
            ValidFeedback(text, acceptedDataPolicy: false));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Data policy needs to be accepted");
        Factory.Mail.Mails.Should().NotContain(m => m.Body.Contains(text));
    }

    [Test]
    public async Task SubmitFeedback_WithOverlongText_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.PostAsJsonAsync(
            $"/api/feedback?auditId={Guid.NewGuid()}",
            ValidFeedback(new string('x', 3001)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Feedback cannot exceed 3000 characters.");
    }

    [Test]
    public async Task SubmitFeedback_WithInvalidEmail_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.PostAsJsonAsync(
            $"/api/feedback?auditId={Guid.NewGuid()}",
            ValidFeedback(NewText(), email: "not-an-email"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Email address is not valid.");
    }

    [Test]
    public async Task SubmitFeedback_WhenConsentCannotBeStored_ReturnsServerErrorAndSendsNothing()
    {
        using var client = Factory.CreateUniqueClient();
        var text = NewText();

        var response = await client.PostAsJsonAsync(
            $"/api/feedback?auditId={Guid.NewGuid()}",
            ValidFeedback(text, email: ControllerApiFactory.EmailConsentFails));

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        Factory.Mail.Mails.Should().NotContain(m => m.Body.Contains(text));
    }
}
