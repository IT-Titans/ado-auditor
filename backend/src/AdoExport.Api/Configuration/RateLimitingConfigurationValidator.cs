using FluentValidation;

using JetBrains.Annotations;

namespace AdoExport.Api.Configuration;

[UsedImplicitly(Reason = "Registered to the DI automatically")]
public class RateLimitingConfigurationValidator : AbstractValidator<RateLimitingConfiguration>
{
    /// <summary>
    /// The shortest shared secret that is accepted for <see cref="RateLimitingConfiguration.TrustedProxyKey" />.
    /// </summary>
    public const int MinimumTrustedProxyKeyLength = 32;

    public RateLimitingConfigurationValidator()
    {
        RuleFor(x => x.Window)
            .GreaterThan(TimeSpan.Zero)
            .LessThanOrEqualTo(TimeSpan.FromHours(1));

        // A limit of zero would reject every request, which is a configuration mistake
        // rather than a sensible setting. Switching the feature off is what Enabled is
        // for, and that stays visible in the configuration.
        RuleFor(x => x.MailPermitLimit).GreaterThan(0);
        RuleFor(x => x.AuditPermitLimit).GreaterThan(0);
        RuleFor(x => x.GlobalPermitLimit).GreaterThan(0);

        RuleFor(x => x.QueueLimit).GreaterThanOrEqualTo(0);

        // Whoever knows this key can choose the address a request is counted under, and
        // with that sidestep every limit. A short key is refused at start instead of being
        // accepted and guessed later. An empty one is fine: it switches the trust off.
        RuleFor(x => x.TrustedProxyKey)
            .MinimumLength(MinimumTrustedProxyKeyLength)
            .When(x => string.IsNullOrWhiteSpace(x.TrustedProxyKey) == false);
    }
}
