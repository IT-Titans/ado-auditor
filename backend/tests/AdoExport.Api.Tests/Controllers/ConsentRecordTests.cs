using System.Net;
using System.Net.Http.Json;

using AdoExport.Api.Tests.Controllers.Support;

using Azure.Communication.Email;

namespace AdoExport.Api.Tests.Controllers;

/// <summary>
/// Nothing a user agreed to may happen without the record that they agreed.
/// </summary>
/// <remarks>
/// The record of consent is a mail. Before these cases existed, storing it reported
/// success whether the mail went out or not, and an audit against a customer's Azure
/// DevOps organization ran without any proof of the consent it rests on. The whole chain
/// runs here, from the request down to the client that would talk to the mail service.
/// </remarks>
public class ConsentRecordTests
{
    private static object AuditRequest() => new
    {
        organization = "contoso",
        pat = "a-personal-access-token",
        email = "someone@example.com",
        acceptedDataPolicy = true,
        acceptedAnonymousAuditLogging = false,
    };

    private static object ContactRequest() => new
    {
        acceptedDataPolicy = true,
        email = "someone@example.com",
        message = "Is the audit result cached anywhere?",
    };

    private static object FeedbackRequest() => new
    {
        acceptedDataPolicy = true,
        name = "Someone",
        email = "someone@example.com",
        feedback = "The report is easy to read.",
    };

    private static string FeedbackRoute() => $"/api/feedback?auditId={Guid.NewGuid()}";

    /// <summary>
    /// The two ways a send can fail to go through: the status the service reports, and
    /// the exception the SDK throws.
    /// </summary>
    private static IEnumerable<TestCaseData> EveryWayTheConsentMailCanFail()
    {
        yield return new TestCaseData((Action<ConsentRecordApiFactory.FakeEmailClient>)(client =>
                client.EndWith(ConsentRecordApiFactory.ConsentSubject, EmailSendStatus.Failed)))
            .SetName("{m}(status Failed)");

        yield return new TestCaseData((Action<ConsentRecordApiFactory.FakeEmailClient>)(client =>
                client.EndWith(ConsentRecordApiFactory.ConsentSubject, EmailSendStatus.Canceled)))
            .SetName("{m}(status Canceled)");

        yield return new TestCaseData((Action<ConsentRecordApiFactory.FakeEmailClient>)(client =>
                client.Throw(ConsentRecordApiFactory.ConsentSubject)))
            .SetName("{m}(RequestFailedException)");
    }

    // --- starting an audit ---

    [Test]
    public async Task StartsTheAuditOnceTheConsentRecordWentOut()
    {
        using var factory = new ConsentRecordApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/audit", AuditRequest());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.AuditService.Started.Should().Be(1);
        factory.EmailClient.SentSubjects.Should().Contain(subject => subject.Contains(ConsentRecordApiFactory.ConsentSubject));
    }

    [TestCaseSource(nameof(EveryWayTheConsentMailCanFail))]
    public async Task DoesNotStartTheAuditWithoutAConsentRecord(Action<ConsentRecordApiFactory.FakeEmailClient> failTheConsentMail)
    {
        using var factory = new ConsentRecordApiFactory();
        failTheConsentMail(factory.EmailClient);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/audit", AuditRequest());

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        factory.AuditService.Started.Should().Be(0, "the personal access token must not be used without proof of consent");
        factory.EmailClient.SentSubjects.Should().BeEmpty("nothing else may be sent about a request that never ran");
    }

    [Test]
    public async Task StillAnswersTheAuditWhenTheMetricsMailFails()
    {
        // The metrics are for the operators. A user must not get an error for an audit
        // that worked because a mail to us did not go out.
        using var factory = new ConsentRecordApiFactory();
        factory.EmailClient.Throw(ConsentRecordApiFactory.MetricsSubject);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/audit", AuditRequest());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.AuditService.Started.Should().Be(1);
    }

    // --- the contact form ---

    [Test]
    public async Task SendsTheContactInquiryOnceTheConsentRecordWentOut()
    {
        using var factory = new ConsentRecordApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/contact", ContactRequest());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.EmailClient.SentSubjects.Should().HaveCount(2);
        factory.EmailClient.SentSubjects.Should().Contain(subject => subject.Contains("Contact Inquiry"));
    }

    [TestCaseSource(nameof(EveryWayTheConsentMailCanFail))]
    public async Task DoesNotSendTheContactInquiryWithoutAConsentRecord(Action<ConsentRecordApiFactory.FakeEmailClient> failTheConsentMail)
    {
        using var factory = new ConsentRecordApiFactory();
        failTheConsentMail(factory.EmailClient);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/contact", ContactRequest());

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        factory.EmailClient.SentSubjects.Should().BeEmpty();
    }

    [Test]
    public async Task TellsTheSenderWhenTheContactInquiryItselfCouldNotBeSent()
    {
        using var factory = new ConsentRecordApiFactory();
        factory.EmailClient.Throw("Contact Inquiry");
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/contact", ContactRequest());

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError, "an inquiry that reached nobody must not be confirmed");
    }

    // --- the feedback form ---

    [Test]
    public async Task SendsTheFeedbackOnceTheConsentRecordWentOut()
    {
        using var factory = new ConsentRecordApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(FeedbackRoute(), FeedbackRequest());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.EmailClient.SentSubjects.Should().HaveCount(2);
        factory.EmailClient.SentSubjects.Should().Contain(subject => subject.Contains("Feedback"));
    }

    [TestCaseSource(nameof(EveryWayTheConsentMailCanFail))]
    public async Task DoesNotSendTheFeedbackWithoutAConsentRecord(Action<ConsentRecordApiFactory.FakeEmailClient> failTheConsentMail)
    {
        using var factory = new ConsentRecordApiFactory();
        failTheConsentMail(factory.EmailClient);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(FeedbackRoute(), FeedbackRequest());

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        factory.EmailClient.SentSubjects.Should().BeEmpty();
    }

    [Test]
    public async Task TellsTheSenderWhenTheFeedbackItselfCouldNotBeSent()
    {
        using var factory = new ConsentRecordApiFactory();
        factory.EmailClient.Throw("Feedback");
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(FeedbackRoute(), FeedbackRequest());

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError, "feedback that reached nobody must not be confirmed");
    }
}
