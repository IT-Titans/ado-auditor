using AdoExport.Auditing.Rules;
using AdoExport.Auditing.Rules.Pats;
using AdoExport.Auditing.Rules.Pats.Reasons;
using AdoExport.Auditing.Rules.SshKeys;
using AdoExport.Auditing.Rules.SshKeys.Reasons;
using AdoExport.Auditing.Rules.Users;
using AdoExport.Auditing.Rules.Users.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Tests.Rules;

/// <summary>
/// Verifies which rules make up each rule set and in which order. A rule that is dropped from a set
/// silently stops being reported.
/// </summary>
[TestFixture]
public class AuditRuleSetTests
{
    private sealed class TestRules : AuditRules<string>
    {
        public TestRules(params IAuditRule<string>[] rules)
        {
            Rules = rules;
        }
    }

    private sealed class FixedRule(AuditRuleReason reason) : IAuditRule<string>
    {
        public RuleResult Audit(string auditee) => new() { Reason = reason };
    }

    [Test]
    public void Audit_WithoutRules_ReturnsEmptyResultForTheAuditee()
    {
        var result = new TestRules().Audit("auditee");

        result.Auditee.Should().Be("auditee");
        result.RuleResults.Should().BeEmpty();
    }

    [Test]
    public void Audit_WithSeveralRules_ReturnsOneResultPerRuleInOrder()
    {
        var first = new PatHasAppScopeReason(new SessionTokenDto());
        var second = new PatHasLimitedScopeReason(new SessionTokenDto());

        var result = new TestRules(new FixedRule(first), new FixedRule(second)).Audit("auditee");

        result.RuleResults.Select(r => r.Reason).Should().Equal(first, second);
    }

    [Test]
    public void PersonalAccessTokenAuditRules_ApplyExpiryScopeAndOrganizationRulesInOrder()
    {
        var token = new SessionTokenDto
        {
            ValidFrom = "2025-01-01T00:00:00Z",
            ValidTo = "2025-12-31T00:00:00Z",
            Scope = "app_token",
            TargetAccounts = null,
        };

        var result = new PersonalAccessTokenAuditRules(EmptyData()).Audit(token);

        result.Auditee.Should().BeSameAs(token);
        result.RuleResults.Select(r => r.Reason.GetType()).Should().Equal(
            typeof(PatExpiresInOverHalfAYearReason),
            typeof(PatHasAppScopeReason),
            typeof(PatIsValidForAllOrganizationsReason));
    }

    [Test]
    public void SshKeysAuditRules_ApplyExpiryAndOrganizationRulesInOrder()
    {
        var key = new SessionTokenDto
        {
            ValidFrom = "2025-01-01T00:00:00Z",
            ValidTo = "2026-01-01T00:00:00Z",
            TargetAccounts = ["a", "b"],
        };

        var result = new SshKeysAuditRules(EmptyData()).Audit(key);

        result.RuleResults.Select(r => r.Reason.GetType()).Should().Equal(
            typeof(SshKeyExpiresInOverAYearReason),
            typeof(SshKeyIsValidForMoreThanOneOrganizationReason));
    }

    [Test]
    public void UserAuditRules_ApplyTypePatLimitAndAdminRulesInOrder()
    {
        var user = new UserDto { Descriptor = "u1", MetaType = UserDto.GuestUserMetaType };

        var result = new UserAuditRules(EmptyData()).Audit(user);

        result.RuleResults.Select(r => r.Reason.GetType()).Should().Equal(
            typeof(UserIsOfTypeGuestReason),
            typeof(UserIsWithinOverallPatLimitReason),
            typeof(UserIsNotOrganizationAdminReason));
    }

    [Test]
    public void Rules_RejectMissingAuditData()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new UserTypeAuditRule(null!));
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
