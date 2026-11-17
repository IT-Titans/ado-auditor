using AdoExport.Client.AdoApi.Errors;

using FluentResults;

namespace AdoExport.Client.Tests.AdoApi.Errors;

public class ErrorTypesTests
{
    [Test]
    public void InvalidPatError_constructors_carry_message_and_cause()
    {
        var cause = new Error("inner");

        new InvalidPatError().Message.Should().BeNull();
        new InvalidPatError("m").Message.Should().Be("m");
        new InvalidPatError("m", cause).Reasons.Should().Contain(cause);
    }

    [Test]
    public void InsufficientRightsError_constructors_carry_message_and_cause()
    {
        var cause = new Error("inner");

        new InsufficientRightsError().Message.Should().BeNull();
        new InsufficientRightsError("m").Message.Should().Be("m");
        new InsufficientRightsError("m", cause).Reasons.Should().Contain(cause);
    }

    [Test]
    public void OrganizationNotFoundError_constructors_carry_message_and_cause()
    {
        var cause = new Error("inner");

        new OrganizationNotFoundError().Message.Should().BeNull();
        new OrganizationNotFoundError("m").Message.Should().Be("m");
        new OrganizationNotFoundError("m", cause).Reasons.Should().Contain(cause);
    }
}
