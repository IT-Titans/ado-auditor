using AdoExport.Auditing.Rules;
using AdoExport.Auditing.Rules.Pats.Reasons;
using AdoExport.Auditing.Rules.Users.Reasons;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Tests;

[TestFixture]
public class AuditModelTests
{
    [Test]
    public void Audit_WhenMetadataIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new Audit(null!)
        {
            PatAuditResults = [],
            SshKeysAuditResults = [],
            UserAuditResults = [],
        });
    }

    [Test]
    public void Audit_TakesIdAndOrganizationFromMetadata()
    {
        var metadata = new AuditMetadata
        {
            AuditId = Guid.NewGuid(),
            OrganizationName = "fabrikam",
            ExpiresAfter = TimeSpan.FromDays(1),
        };

        var audit = new Audit(metadata)
        {
            PatAuditResults = [],
            SshKeysAuditResults = [],
            UserAuditResults = [],
        };

        audit.AuditId.Should().Be(metadata.AuditId);
        audit.OrganizationName.Should().Be("fabrikam");
        audit.Metadata.Should().BeSameAs(metadata);
    }

    [Test]
    public void RuleHint_WhenDescriptionAndRecommendationsAreMissing_FallsBackToPlaceholders()
    {
        var hint = new RuleHint { Name = "rule" };

        hint.Description.Should().Contain("Coming soon");
        hint.Recommendations.Should().Contain("Coming soon");
    }

    [Test]
    public void RuleHint_WhenDescriptionAndRecommendationsAreSet_ReturnsThem()
    {
        var hint = new RuleHint { Name = "rule", Description = "what", Recommendations = "how" };

        hint.Name.Should().Be("rule");
        hint.Description.Should().Be("what");
        hint.Recommendations.Should().Be("how");
    }

    [Test]
    public void GetTotalRuleResultsBySeverity_WhenAuditResultsIsNull_Throws()
    {
        IEnumerable<AuditResult<UserDto>> results = null!;

        Assert.Throws<ArgumentNullException>(() => results.GetTotalRuleResultsBySeverity(Severities.Critical));
    }

    [TestCase(Severities.Critical, 1)]
    [TestCase(Severities.Warning, 2)]
    [TestCase(Severities.Ok, 3)]
    public void GetTotalRuleResultsBySeverity_CountsOnlyRuleResultsOfThatSeverityAcrossAllResults(Severities severity, int expected)
    {
        var user = new UserDto();
        AuditResult<UserDto>[] results =
        [
            new() { Auditee = user, RuleResults = [Result(new UserIsOfTypeUnknownReason(user)), Result(new UserIsOfTypeGuestReason(user))] },
            new() { Auditee = user, RuleResults = [Result(new UserIsOrganizationAdminReason(user)), Result(new UserIsOfTypeMemberReason(user))] },
            new() { Auditee = user, RuleResults = [Result(new UserIsNotOrganizationAdminReason(user)), Result(new UserIsWithinOverallPatLimitReason(user))] },
        ];

        results.GetTotalRuleResultsBySeverity(severity).Should().Be(expected);
    }

    [Test]
    public void GetTotalRuleResultsBySeverity_WithNoResults_ReturnsZero()
    {
        AuditResult<SessionTokenDto>[] results = [];

        results.GetTotalRuleResultsBySeverity(Severities.Critical).Should().Be(0);
    }

    [Test]
    public void GetTotalRuleResultsBySeverity_WithResultWithoutRuleResults_ReturnsZero()
    {
        AuditResult<SessionTokenDto>[] results = [new() { Auditee = new SessionTokenDto(), RuleResults = [] }];

        results.GetTotalRuleResultsBySeverity(Severities.Critical).Should().Be(0);
    }

    [Test]
    public void GetTotalRuleResultsBySeverity_WithOneMatchingResult_ReturnsOne()
    {
        var token = new SessionTokenDto();
        AuditResult<SessionTokenDto>[] results =
        [
            new() { Auditee = token, RuleResults = [Result(new PatHasAppScopeReason(token))] },
        ];

        results.GetTotalRuleResultsBySeverity(Severities.Critical).Should().Be(1);
        results.GetTotalRuleResultsBySeverity(Severities.Warning).Should().Be(0);
    }

    private static RuleResult Result(AuditRuleReason reason) => new() { Reason = reason };
}
