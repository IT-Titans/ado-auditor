using AdoExport.Common.Errors;

using FluentResults;

namespace AdoExport.Common.Tests.Errors;

[TestFixture]
public class RequestWasCanceledErrorTests
{
    [Test]
    public void Constructor_WithoutCause_HasNoReasons()
    {
        var error = new RequestWasCanceledError();

        error.Reasons.Should().BeEmpty();
    }

    [Test]
    public void Constructor_WithCause_DescribesTheCancellationAndKeepsTheCause()
    {
        var cause = new Error("socket closed");

        var error = new RequestWasCanceledError(cause);

        error.Message.Should().Be("User canceled request");
        error.Reasons.Should().ContainSingle().Which.Should().BeSameAs(cause);
    }

    [Test]
    public void RequestWasCanceled_WhenCancellationIsNestedAsCauseOfAnotherError_IsDetected()
    {
        // HasError searches nested reasons, so wrapping must still be detected.
        var wrapped = Result.Fail(new Error("outer").CausedBy(new RequestWasCanceledError()));

        wrapped.RequestWasCanceled().Should().BeTrue();
    }

    [Test]
    public void RequestWasCanceled_WhenCancellationIsOneOfSeveralErrors_IsTrue()
    {
        var result = Result.Fail(new Error("first")).WithError(new RequestWasCanceledError());

        result.RequestWasCanceled().Should().BeTrue();
    }

    [Test]
    public void RequestWasCanceled_WorksForGenericResults()
    {
        Result.Fail<int>(new RequestWasCanceledError()).RequestWasCanceled().Should().BeTrue();
        Result.Ok(5).RequestWasCanceled().Should().BeFalse();
    }
}
