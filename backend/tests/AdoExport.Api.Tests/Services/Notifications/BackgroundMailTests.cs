using AdoExport.Api.Services.Notifications;
using AdoExport.Api.Tests.Repositories;

using FluentResults;

using Microsoft.Extensions.Logging;

namespace AdoExport.Api.Tests.Services.Notifications;

/// <summary>
/// A mail nobody waits for may fail, but not without a trace and never into the request
/// that started it.
/// </summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class BackgroundMailTests
{
    private readonly CapturingLogger<BackgroundMailTests> _logger = new();

    [Test]
    public async Task LogsNothingWhenTheMailWasSent()
    {
        await BackgroundMail.Send(() => Task.FromResult(Result.Ok()), _logger, "audit metrics");

        _logger.Entries.Should().BeEmpty();
    }

    [Test]
    public async Task LogsAFailedSendAsAnError()
    {
        await BackgroundMail.Send(() => Task.FromResult(Result.Fail("mail service unavailable")), _logger, "audit metrics");

        _logger.Entries
            .Should()
            .ContainSingle(entry => entry.Level == LogLevel.Error)
            .Which.Message.Should().Contain("audit metrics").And.Contain("mail service unavailable");
    }

    [Test]
    public async Task LogsAnExceptionFromTheSendInsteadOfLosingIt()
    {
        // A discarded task takes its exception with it. This is the case that used to
        // vanish without anyone ever seeing it.
        Func<Task<Result>> send = async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        };

        var act = () => BackgroundMail.Send(send, _logger, "error report");

        await act.Should().NotThrowAsync();
        _logger.Entries
            .Should()
            .ContainSingle(entry => entry.Level == LogLevel.Error)
            .Which.Message.Should().Contain("error report");
    }

    [Test]
    public async Task KeepsAnExceptionThrownWhileStartingTheSendAwayFromTheCaller()
    {
        // The send is started in a finally block and in the exception handler. An
        // exception escaping from there would replace the response the user is owed.
        Func<Task<Result>> send = () => throw new ArgumentException("receiver is missing");

        var act = () => BackgroundMail.Send(send, _logger, "audit metrics");

        await act.Should().NotThrowAsync();
        _logger.Entries.Should().ContainSingle(entry => entry.Level == LogLevel.Error);
    }

    [Test]
    public async Task ToleratesASendThatReportsNothing()
    {
        await BackgroundMail.Send(() => Task.FromResult<Result>(null!), _logger, "audit metrics");

        _logger.Entries.Should().BeEmpty();
    }

    [Test]
    public async Task RefusesAMissingSend()
    {
        var act = () => BackgroundMail.Send(null!, _logger, "audit metrics");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task RefusesAMissingLogger()
    {
        var act = () => BackgroundMail.Send(() => Task.FromResult(Result.Ok()), null!, "audit metrics");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [TestCase("")]
    [TestCase("   ")]
    public async Task RefusesAMailWithoutAName(string description)
    {
        var act = () => BackgroundMail.Send(() => Task.FromResult(Result.Ok()), _logger, description);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
