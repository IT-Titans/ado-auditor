using System.Text.Json;

using AdoExport.Api.Configuration;
using AdoExport.Api.Services.Notifications;
using AdoExport.Api.Services.UserConsent;

using FluentResults;

using Microsoft.Extensions.Options;

namespace AdoExport.Api.Tests.Services;

/// <summary>
/// The record of a user's consent is a mail. These cases make sure that "stored" means
/// the mail went out, for each of the three places consent is given.
/// </summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class UserConsentServiceTests
{
    private const string Sender = "DoNotReply@example.com";

    private const string Receiver = "consent@example.com";

    private const string AuditId = "5f0b6b6a-52b3-4a8e-9a0e-0f1f6f6f1c11";

    private const string UserIdentifier = "someone@example.com";

    private static readonly DateTime TimeOfConsent = new(2026, 11, 17, 9, 30, 0, DateTimeKind.Utc);

    private readonly RecordingSendMailService _mail = new();

    private UserConsentService CreateSut()
        => new(
            _mail,
            Options.Create(new UserConsentMailConfiguration { Sender = Sender, Receiver = Receiver }));

    private static IEnumerable<TestCaseData> EveryWayOfStoringConsent()
    {
        yield return new TestCaseData(
                (Func<IUserConsentService, Task<Result>>)(sut => sut.StoreUserConsentForContactFormAsync(UserIdentifier, TimeOfConsent)))
            .SetName("{m}(contact form)");

        yield return new TestCaseData(
                (Func<IUserConsentService, Task<Result>>)(sut => sut.StoreUserConsentForFeedbackFormAsync(AuditId, UserIdentifier, TimeOfConsent)))
            .SetName("{m}(feedback form)");

        yield return new TestCaseData(
                (Func<IUserConsentService, Task<Result>>)(sut => sut.StoreUserConsentForAuditAsync(AuditId, UserIdentifier, true, TimeOfConsent)))
            .SetName("{m}(start audit form)");
    }

    [TestCaseSource(nameof(EveryWayOfStoringConsent))]
    public async Task SucceedsWhenTheRecordWasSent(Func<IUserConsentService, Task<Result>> store)
    {
        var result = await store(CreateSut());

        result.IsSuccess.Should().BeTrue();
        _mail.Mails.Should().ContainSingle();
    }

    [TestCaseSource(nameof(EveryWayOfStoringConsent))]
    public async Task FailsWhenTheRecordCouldNotBeSent(Func<IUserConsentService, Task<Result>> store)
    {
        // This used to answer "stored" whatever happened to the mail, so a request went
        // ahead without any record that the user had agreed to it.
        _mail.Outcome = Result.Fail("mail service unavailable");

        var result = await store(CreateSut());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be("mail service unavailable");
    }

    [TestCaseSource(nameof(EveryWayOfStoringConsent))]
    public async Task SendsTheRecordFromAndToTheConfiguredAddresses(Func<IUserConsentService, Task<Result>> store)
    {
        await store(CreateSut());

        var mail = _mail.Mails.Should().ContainSingle().Subject;
        mail.Sender.Should().Be(Sender);
        mail.Receiver.Should().Be(Receiver);
        mail.Subject.Should().Contain("User Consent");
    }

    [Test]
    public async Task RecordsWhoAgreedAndWhenForTheContactForm()
    {
        await CreateSut().StoreUserConsentForContactFormAsync(UserIdentifier, TimeOfConsent);

        var record = SentRecord();
        record.GetProperty("userIdentifier").GetString().Should().Be(UserIdentifier);
        record.GetProperty("consentTimestamp").GetDateTime().Should().Be(TimeOfConsent);
        record.GetProperty("consentOrigin").GetString().Should().Be("contactForm");
    }

    [Test]
    public async Task RecordsTheAuditTheFeedbackBelongsTo()
    {
        await CreateSut().StoreUserConsentForFeedbackFormAsync(AuditId, UserIdentifier, TimeOfConsent);

        var record = SentRecord();
        record.GetProperty("userIdentifier").GetString().Should().Be(UserIdentifier);
        record.GetProperty("auditIdentifier").GetString().Should().Be(AuditId);
        record.GetProperty("consentOrigin").GetString().Should().Be("feedbackForm");
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task RecordsWhetherAnonymousLoggingWasAgreedToForAnAudit(bool consentForAnonymousLogging)
    {
        await CreateSut().StoreUserConsentForAuditAsync(AuditId, UserIdentifier, consentForAnonymousLogging, TimeOfConsent);

        var record = SentRecord();
        record.GetProperty("auditIdentifier").GetString().Should().Be(AuditId);
        record.GetProperty("consentToAnonymousLogging").GetBoolean().Should().Be(consentForAnonymousLogging);
        record.GetProperty("consentOrigin").GetString().Should().Be("startAuditForm");
    }

    [Test]
    public void RefusesAMissingMailService()
    {
        var act = () => new UserConsentService(
            null!,
            Options.Create(new UserConsentMailConfiguration { Sender = Sender, Receiver = Receiver }));

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RefusesMissingOptions()
    {
        var act = () => new UserConsentService(_mail, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    private JsonElement SentRecord()
        => JsonDocument.Parse(_mail.Mails.Should().ContainSingle().Subject.Body).RootElement;

    private sealed class RecordingSendMailService : ISendMailService
    {
        public List<(string Sender, string Receiver, string Subject, string Body)> Mails { get; } = [];

        public Result Outcome { get; set; } = Result.Ok();

        public Task<Result> SendMailAsync(string sender, string receiver, string subject, string body)
        {
            Mails.Add((sender, receiver, subject, body));

            return Task.FromResult(Outcome);
        }
    }
}
