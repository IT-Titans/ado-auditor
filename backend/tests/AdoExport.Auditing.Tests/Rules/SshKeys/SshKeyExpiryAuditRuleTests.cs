using System.Globalization;

using AdoExport.Auditing.Rules.SshKeys;
using AdoExport.Auditing.Rules.SshKeys.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Tests.Rules.SshKeys;

[TestFixture]
public class SshKeyExpiryAuditRuleTests
{
    private static readonly DateTime ValidFrom = new(2025, 8, 8, 10, 0, 0, DateTimeKind.Utc);

    private static IEnumerable<TestCaseData> ExpiryTestCaseSource()
    {
        // Short lifetime is fine.
        yield return new TestCaseData(0, typeof(SshKeyExpiresWithinShortTimeRangeReason), Severities.Ok);
        yield return new TestCaseData(1, typeof(SshKeyExpiresWithinShortTimeRangeReason), Severities.Ok);
        yield return new TestCaseData(181, typeof(SshKeyExpiresWithinShortTimeRangeReason), Severities.Ok);

        // 365 / 2 = 182 days: the lower bound is inclusive.
        yield return new TestCaseData(182, typeof(SshKeyExpiresInOverHalfAYearReason), Severities.Warning);
        yield return new TestCaseData(183, typeof(SshKeyExpiresInOverHalfAYearReason), Severities.Warning);
        yield return new TestCaseData(364, typeof(SshKeyExpiresInOverHalfAYearReason), Severities.Warning);

        // 365 days: the lower bound is inclusive.
        yield return new TestCaseData(365, typeof(SshKeyExpiresInOverAYearReason), Severities.Critical);
        yield return new TestCaseData(366, typeof(SshKeyExpiresInOverAYearReason), Severities.Critical);
        yield return new TestCaseData(3650, typeof(SshKeyExpiresInOverAYearReason), Severities.Critical);
    }

    [TestCaseSource(nameof(ExpiryTestCaseSource))]
    public void Audit_ReturnsExpectedReasonForGivenLifetime(int daysValid, Type expectedType, Severities expectedSeverity)
    {
        var sut = CreateSut();

        var ruleResult = sut.Audit(CreateKey(ValidFrom, ValidFrom.AddDays(daysValid)));

        ruleResult.AssertRuleResult(expectedType, Categories.Pats, expectedSeverity);
    }

    [Test]
    public void Audit_WhenLifetimeIsOneHourShortOfTheYearBoundary_IsNotCritical()
    {
        var sut = CreateSut();

        var ruleResult = sut.Audit(CreateKey(ValidFrom, ValidFrom.AddDays(365).AddHours(-1)));

        ruleResult.AssertRuleResult<SshKeyExpiresInOverHalfAYearReason>(Categories.Pats, Severities.Warning);
    }

    [Test]
    public void Audit_WhenLifetimeIsOneHourShortOfTheHalfYearBoundary_IsOk()
    {
        var sut = CreateSut();

        var ruleResult = sut.Audit(CreateKey(ValidFrom, ValidFrom.AddDays(182).AddHours(-1)));

        ruleResult.AssertRuleResult<SshKeyExpiresWithinShortTimeRangeReason>(Categories.Pats, Severities.Ok);
    }

    [Test]
    public void Audit_WhenTimestampsCarryUtcOffsets_MeasuresTheRealElapsedTime()
    {
        var sut = CreateSut();

        // Exactly 182 days apart, written once in UTC and once with an offset.
        var key = new SessionTokenDto
        {
            ValidFrom = "2025-01-01T00:00:00+00:00",
            ValidTo = "2025-07-02T02:00:00+02:00",
            TargetAccounts = [],
        };

        sut.Audit(key).AssertRuleResult<SshKeyExpiresInOverHalfAYearReason>(Categories.Pats, Severities.Warning);
    }

    /// <summary>
    /// The lifetime used to be subtracted from local wall-clock values, which shifts it by an hour across
    /// a daylight saving change: a key a few minutes short of the boundary counted as on it, and a key
    /// exactly on it as below it. Skipped on machines whose time zone has no such change in the span.
    /// </summary>
    [TestCase(1, 1, 182, -30, typeof(SshKeyExpiresWithinShortTimeRangeReason))]
    [TestCase(1, 1, 182, 0, typeof(SshKeyExpiresInOverHalfAYearReason))]
    [TestCase(7, 1, 182, -30, typeof(SshKeyExpiresWithinShortTimeRangeReason))]
    [TestCase(7, 1, 182, 0, typeof(SshKeyExpiresInOverHalfAYearReason))]
    public void Audit_AcrossDaylightSavingChange_MeasuresRealElapsedTime(int month, int day, int days, int minutesOffset, Type expectedType)
    {
        var from = new DateTime(2025, month, day, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddDays(days).AddMinutes(minutesOffset);
        var zone = TimeZoneInfo.Local;
        Assume.That(zone.GetUtcOffset(from), Is.Not.EqualTo(zone.GetUtcOffset(to)), "No daylight saving change within the span in this time zone.");

        var ruleResult = CreateSut().Audit(CreateKey(from, to));

        ruleResult.Reason.GetType().Should().Be(expectedType);
    }

    [Test]
    public void Audit_WhenAuditeeIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => CreateSut().Audit(null!));
    }

    [Test]
    public void Audit_WhenValidToIsNotADate_Throws()
    {
        var sut = CreateSut();
        var key = CreateKey(ValidFrom, ValidFrom.AddDays(10));
        key.ValidTo = "never";

        Assert.Throws<FormatException>(() => sut.Audit(key));
    }

    private static SshKeyExpiryAuditRule CreateSut()
    {
        return new SshKeyExpiryAuditRule(new Fixture().Create<RawAuditData>());
    }

    private static SessionTokenDto CreateKey(DateTime validFrom, DateTime validTo)
    {
        return new SessionTokenDto
        {
            ValidFrom = validFrom.ToString("o", CultureInfo.InvariantCulture),
            ValidTo = validTo.ToString("o", CultureInfo.InvariantCulture),
            TargetAccounts = [],
        };
    }
}
