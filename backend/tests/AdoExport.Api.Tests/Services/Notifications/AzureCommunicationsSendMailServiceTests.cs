using AdoExport.Api.Services.Notifications;
using AdoExport.Api.Tests.Repositories;

using Azure;
using Azure.Communication.Email;

using FluentResults;

using Microsoft.Extensions.Logging;

namespace AdoExport.Api.Tests.Services.Notifications;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class AzureCommunicationsSendMailServiceTests
{
    private const string Sender = "DoNotReply@example.com";

    private const string Receiver = "consent@example.com";

    private const string Subject = "User Consent";

    private const string Body = "{}";

    private readonly CapturingLogger<AzureCommunicationsSendMailService> _logger = new();

    private readonly Mock<IEmailClient> _emailClient = new();

    private AzureCommunicationsSendMailService CreateSut() => new(_logger, _emailClient.Object);

    private void TheClientAnswers(EmailSendStatus status)
        => _emailClient
            .Setup(client => client.SendAsync(Sender, Receiver, Subject, Body))
            .ReturnsAsync(status);

    private void TheClientThrows(Exception exception)
        => _emailClient
            .Setup(client => client.SendAsync(Sender, Receiver, Subject, Body))
            .ThrowsAsync(exception);

    [Test]
    public async Task SucceedsWhenTheMailWasSent()
    {
        TheClientAnswers(EmailSendStatus.Succeeded);

        var result = await CreateSut().SendMailAsync(Sender, Receiver, Subject, Body);

        result.IsSuccess.Should().BeTrue();
        _logger.Entries.Should().NotContain(entry => entry.Level >= LogLevel.Warning);
    }

    [Test]
    public async Task HandsSenderReceiverSubjectAndBodyToTheClientUnchanged()
    {
        TheClientAnswers(EmailSendStatus.Succeeded);

        await CreateSut().SendMailAsync(Sender, Receiver, Subject, Body);

        _emailClient.Verify(client => client.SendAsync(Sender, Receiver, Subject, Body), Times.Once);
    }

    private static IEnumerable<EmailSendStatus> EveryStatusButSucceeded()
    {
        yield return EmailSendStatus.Failed;
        yield return EmailSendStatus.Canceled;
        yield return EmailSendStatus.Running;
        yield return EmailSendStatus.NotStarted;
        yield return new EmailSendStatus("SomethingTheServiceAddsLater");
    }

    [TestCaseSource(nameof(EveryStatusButSucceeded))]
    public async Task FailsOnEveryStatusOtherThanSucceeded(EmailSendStatus status)
    {
        // Success is the only status that means the mail is on its way. Anything else,
        // including a status this code has never heard of, must not read as sent.
        TheClientAnswers(status);

        var result = await CreateSut().SendMailAsync(Sender, Receiver, Subject, Body);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Contain(status.ToString());
    }

    [Test]
    public async Task LogsAStatusOtherThanSucceededAsAnError()
    {
        TheClientAnswers(EmailSendStatus.Failed);

        await CreateSut().SendMailAsync(Sender, Receiver, Subject, Body);

        _logger.Entries
            .Should()
            .ContainSingle(entry => entry.Level == LogLevel.Error)
            .Which.Message.Should().Contain(Subject).And.Contain("Failed");
    }

    [Test]
    public async Task FailsWhenTheClientThrowsARequestFailedException()
    {
        // The SDK reports a rejected request, an unreachable service and a send that
        // ended as failed this way. None of them may escape as an unhandled exception
        // and none of them may read as sent.
        var exception = new RequestFailedException(500, "The mail could not be delivered.", "EmailDropped", innerException: null);
        TheClientThrows(exception);

        var result = await CreateSut().SendMailAsync(Sender, Receiver, Subject, Body);

        result.IsFailed.Should().BeTrue();
        result.Errors
            .Should()
            .ContainSingle()
            .Which.Should().BeOfType<ExceptionalError>()
            .Which.Exception.Should().BeSameAs(exception);
    }

    [Test]
    public async Task LogsARequestFailedExceptionAsAnError()
    {
        TheClientThrows(new RequestFailedException(401, "Denied", "Denied", innerException: null));

        await CreateSut().SendMailAsync(Sender, Receiver, Subject, Body);

        _logger.Entries
            .Should()
            .ContainSingle(entry => entry.Level == LogLevel.Error)
            .Which.Message.Should().Contain(Subject).And.Contain("Denied").And.Contain("401");
    }

    [Test]
    public async Task LetsAnyOtherExceptionThrough()
    {
        // A programming error is not a failed send. It should stay loud instead of being
        // filed under "the mail service was unavailable".
        TheClientThrows(new InvalidOperationException("not a mail problem"));

        var act = () => CreateSut().SendMailAsync(Sender, Receiver, Subject, Body);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public void RefusesAMissingLogger()
    {
        var act = () => new AzureCommunicationsSendMailService(null!, _emailClient.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RefusesAMissingClient()
    {
        var act = () => new AzureCommunicationsSendMailService(_logger, null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
