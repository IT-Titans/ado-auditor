using System.Globalization;

using AdoExport.Auditing.Rules.Pats;
using AdoExport.Auditing.Rules.Pats.Reasons;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Tests.Rules.Pats;

[TestFixture]
public class PersonalAccessTokenExpiryAuditRuleTests
{
    [Test]
    public void Audit_WhenAuditeeIsNull_Throws()
    {
        var fixture = CreateFixture();

        var sut = fixture.Create<PersonalAccessTokenExpiryAuditRule>();

        Assert.Throws<ArgumentNullException>(() => sut.Audit(null!));
    }

    private static IEnumerable<TestCaseData> ExpiryTestCaseSource()
    {
        var validFrom = DateTime.Parse("2025-08-08 10:00:00Z").ToUniversalTime();
        yield return new TestCaseData(validFrom, typeof(PatExpiresWithinShortTimeRangeReason), 0, Severities.Ok);
        yield return new TestCaseData(validFrom, typeof(PatExpiresWithinShortTimeRangeReason), 1, Severities.Ok);
        yield return new TestCaseData(validFrom, typeof(PatExpiresWithinShortTimeRangeReason), 29, Severities.Ok);
        yield return new TestCaseData(validFrom, typeof(PatExpiresWithinShortTimeRangeReason), 30, Severities.Ok);
        yield return new TestCaseData(validFrom, typeof(PatExpiresWithinShortTimeRangeReason), 31, Severities.Ok);
        yield return new TestCaseData(validFrom, typeof(PatExpiresWithinShortTimeRangeReason), 59, Severities.Ok);
        yield return new TestCaseData(validFrom, typeof(PatExpiresWithinShortTimeRangeReason), 60, Severities.Ok);
        yield return new TestCaseData(validFrom, typeof(PatExpiresWithinShortTimeRangeReason), 61, Severities.Ok);
        yield return new TestCaseData(validFrom, typeof(PatExpiresWithinShortTimeRangeReason), 89, Severities.Ok);
        yield return new TestCaseData(validFrom, typeof(PatExpiresWithinShortTimeRangeReason), 90, Severities.Ok);
        
        yield return new TestCaseData(validFrom, typeof(PatExpiresInOverOneQuarterReason), 91, Severities.Warning);
        yield return new TestCaseData(validFrom, typeof(PatExpiresInOverOneQuarterReason), 179, Severities.Warning);
        yield return new TestCaseData(validFrom, typeof(PatExpiresInOverOneQuarterReason), 180, Severities.Warning);
        
        yield return new TestCaseData(validFrom, typeof(PatExpiresInOverHalfAYearReason), 181, Severities.Critical);
        yield return new TestCaseData(validFrom, typeof(PatExpiresInOverHalfAYearReason), 182, Severities.Critical);
        yield return new TestCaseData(validFrom, typeof(PatExpiresInOverHalfAYearReason), 354, Severities.Critical);
        yield return new TestCaseData(validFrom, typeof(PatExpiresInOverHalfAYearReason), 355, Severities.Critical);
        yield return new TestCaseData(validFrom, typeof(PatExpiresInOverHalfAYearReason), 356, Severities.Critical);
        yield return new TestCaseData(validFrom, typeof(PatExpiresInOverHalfAYearReason), 357, Severities.Critical);
    }

    [TestCaseSource(nameof(ExpiryTestCaseSource))]
    public void Audit_ReturnsExpectedReasonForGivenExpiry(DateTime validFrom, Type expectedType, int daysValid, Severities expectedSeverity)
    {
        var fixture = CreateFixture();

        var validTo = validFrom.AddDays(daysValid);

        var auditee = fixture.Build<SessionTokenDto>()
            .With(x => x.ValidFrom, validFrom.ToString("u", CultureInfo.InvariantCulture))
            .With(x => x.ValidTo, validTo.ToString("u", CultureInfo.InvariantCulture))
            .Create();

        var sut = fixture.Create<PersonalAccessTokenExpiryAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult(expectedType, Categories.Pats, expectedSeverity);
    }

    private static Fixture CreateFixture()
    {
        return new Fixture();
    }
}