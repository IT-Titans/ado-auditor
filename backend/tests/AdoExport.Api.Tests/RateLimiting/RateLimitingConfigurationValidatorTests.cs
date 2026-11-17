using AdoExport.Api.Configuration;

namespace AdoExport.Api.Tests.RateLimiting;

public class RateLimitingConfigurationValidatorTests
{
    private readonly RateLimitingConfigurationValidator _validator = new();

    [Test]
    public void AcceptsTheDefaults()
    {
        // The section may be missing from configuration entirely, in which case binding
        // produces this object. If the defaults did not validate, the application would
        // refuse to start - ValidateOnStart is in play.
        _validator
            .Validate(new RateLimitingConfiguration())
            .IsValid
            .Should()
            .BeTrue();
    }

    [Test]
    public void LeavesTheLimitsInPlaceWhenNothingIsConfigured()
    {
        var configuration = new RateLimitingConfiguration();

        configuration.Enabled.Should().BeTrue();
        configuration.MailPermitLimit.Should().BeGreaterThan(0);
        configuration.AuditPermitLimit.Should().BeGreaterThan(0);
        configuration.GlobalPermitLimit.Should().BeGreaterThan(0);
    }

    [Test]
    public void KeepsTheMailLimitTighterThanTheAuditLimit()
    {
        // Every accepted mail request lands in an IT Titans mailbox, so the default must
        // not be looser than the one for starting an audit.
        var configuration = new RateLimitingConfiguration();

        configuration
            .MailPermitLimit
            .Should()
            .BeLessThanOrEqualTo(configuration.AuditPermitLimit);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void RejectsALimitThatWouldBlockEveryRequest(int limit)
    {
        // Zero is a configuration mistake, not a way to switch the feature off. Enabled
        // does that, and it stays visible in the configuration.
        _validator
            .Validate(new RateLimitingConfiguration { MailPermitLimit = limit })
            .IsValid
            .Should()
            .BeFalse();
    }

    [Test]
    public void RejectsAWindowOfZero()
    {
        _validator
            .Validate(new RateLimitingConfiguration { Window = TimeSpan.Zero })
            .IsValid
            .Should()
            .BeFalse();
    }

    [Test]
    public void RejectsAWindowSoLongThatAClientIsLockedOutForHours()
    {
        _validator
            .Validate(new RateLimitingConfiguration { Window = TimeSpan.FromHours(2) })
            .IsValid
            .Should()
            .BeFalse();
    }

    [Test]
    public void RejectsANegativeQueueLimit()
    {
        _validator
            .Validate(new RateLimitingConfiguration { QueueLimit = -1 })
            .IsValid
            .Should()
            .BeFalse();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void AcceptsAMissingTrustedProxyKey(string? key)
    {
        // Not setting the key is how the visitor address trust is switched off, and it is
        // the state of every deployment until an operator has set it.
        _validator
            .Validate(new RateLimitingConfiguration { TrustedProxyKey = key })
            .IsValid
            .Should()
            .BeTrue();
    }

    [Test]
    public void RejectsATrustedProxyKeyThatIsTooShortToBeASecret()
    {
        var tooShort = new string('k', RateLimitingConfigurationValidator.MinimumTrustedProxyKeyLength - 1);

        _validator
            .Validate(new RateLimitingConfiguration { TrustedProxyKey = tooShort })
            .IsValid
            .Should()
            .BeFalse();
    }

    [Test]
    public void AcceptsATrustedProxyKeyOfTheMinimumLength()
    {
        var longEnough = new string('k', RateLimitingConfigurationValidator.MinimumTrustedProxyKeyLength);

        _validator
            .Validate(new RateLimitingConfiguration { TrustedProxyKey = longEnough })
            .IsValid
            .Should()
            .BeTrue();
    }
}
