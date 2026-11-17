using System.Globalization;

using AdoExport.Auditing.Rules.Pats;
using AdoExport.Auditing.Rules.Pats.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Tests.Rules.Pats;

/// <summary>
/// Sub-day boundaries and unusual inputs for the PAT rules. The day-granular boundaries live in the
/// expiry rule tests next to this file.
/// </summary>
[TestFixture]
public class PersonalAccessTokenBoundaryTests
{
    private static readonly DateTime ValidFrom = new(2025, 8, 8, 10, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Expiry_OneHourOverTheQuarterBoundary_IsWarning()
    {
        var result = AuditExpiry(ValidFrom, ValidFrom.AddDays(90).AddHours(1));

        result.AssertRuleResult<PatExpiresInOverOneQuarterReason>(Categories.Pats, Severities.Warning);
    }

    [Test]
    public void Expiry_OneHourUnderTheQuarterBoundary_IsOk()
    {
        var result = AuditExpiry(ValidFrom, ValidFrom.AddDays(90).AddHours(-1));

        result.AssertRuleResult<PatExpiresWithinShortTimeRangeReason>(Categories.Pats, Severities.Ok);
    }

    [Test]
    public void Expiry_OneHourOverTheHalfYearBoundary_IsCritical()
    {
        var result = AuditExpiry(ValidFrom, ValidFrom.AddDays(180).AddHours(1));

        result.AssertRuleResult<PatExpiresInOverHalfAYearReason>(Categories.Pats, Severities.Critical);
    }

    [Test]
    public void Expiry_OneHourUnderTheHalfYearBoundary_IsWarning()
    {
        var result = AuditExpiry(ValidFrom, ValidFrom.AddDays(180).AddHours(-1));

        result.AssertRuleResult<PatExpiresInOverOneQuarterReason>(Categories.Pats, Severities.Warning);
    }

    [Test]
    public void Expiry_TimestampsWithDifferentOffsets_AreComparedAsInstants()
    {
        // 90 days and 1 hour apart in absolute time, written with different offsets.
        var token = new SessionTokenDto
        {
            ValidFrom = "2025-01-01T00:00:00+00:00",
            ValidTo = "2025-04-01T03:00:00+02:00",
        };

        var result = new PersonalAccessTokenExpiryAuditRule(EmptyData()).Audit(token);

        result.AssertRuleResult<PatExpiresInOverOneQuarterReason>(Categories.Pats, Severities.Warning);
    }

    [Test]
    public void Expiry_WhenValidToIsNotADate_Throws()
    {
        var token = new SessionTokenDto { ValidFrom = "2025-01-01T00:00:00Z", ValidTo = "never" };

        Assert.Throws<FormatException>(() => new PersonalAccessTokenExpiryAuditRule(EmptyData()).Audit(token));
    }

    [Test]
    public void Scope_WhenScopeIsMissing_IsNotTreatedAsAppToken()
    {
        var result = new PersonalAccessTokenScopesAuditRule(EmptyData()).Audit(new SessionTokenDto { Scope = null });

        result.AssertRuleResult<PatHasLimitedScopeReason>(Categories.Pats, Severities.Ok);
    }

    [Test]
    public void Scope_AppTokenAmongOtherScopes_IsNotTreatedAsAppToken()
    {
        var result = new PersonalAccessTokenScopesAuditRule(EmptyData()).Audit(new SessionTokenDto { Scope = "vso.code app_token" });

        result.AssertRuleResult<PatHasLimitedScopeReason>(Categories.Pats, Severities.Ok);
    }

    private static AdoExport.Auditing.Rules.RuleResult AuditExpiry(DateTime validFrom, DateTime validTo)
    {
        var token = new SessionTokenDto
        {
            ValidFrom = validFrom.ToString("o", CultureInfo.InvariantCulture),
            ValidTo = validTo.ToString("o", CultureInfo.InvariantCulture),
        };

        return new PersonalAccessTokenExpiryAuditRule(EmptyData()).Audit(token);
    }

    private static RawAuditData EmptyData()
    {
        return new RawAuditData
        {
            AllUsers = [],
            UserGroupMemberships = new Dictionary<string, GroupDto[]>(),
            UsersWithProjectEntitlements = [],
            PrivateSessionTokenDetailsWithUser = [],
            PublicSessionTokenDetailsWithUser = [],
        };
    }
}
