using AdoExport.Auditing.Rules.Pats;
using AdoExport.Auditing.Rules.Pats.Reasons;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Tests.Rules.Pats;

[TestFixture]
public class PersonalAccessTokenScopesAuditRuleTests
{
    [Test]
    public void Audit_WhenAuditeeIsNull_Throws()
    {
        var fixture = CreateFixture();

        var sut = fixture.Create<PersonalAccessTokenScopesAuditRule>();

        Assert.Throws<ArgumentNullException>(() =>  sut.Audit(null!));
    }

    private static IEnumerable<TestCaseData> ScopesWithoutAppTokenScope()
    {
        yield return new TestCaseData("not_app_token");
        yield return new TestCaseData("vso.tokenmanagement");
        yield return new TestCaseData("vso.tokenmanagement vso.memberships");
    }

    [TestCaseSource(nameof(ScopesWithoutAppTokenScope))]
    public void Audit_WhenScopesHasNotAppTokenScope_ReturnsPatHasLimitedScopeReason(string scopes)
    {
        var fixture = CreateFixture();

        var auditee = fixture.Build<SessionTokenDto>()
            .With(x => x.Scope, scopes)
            .Create();

        var sut = fixture.Create<PersonalAccessTokenScopesAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult<PatHasLimitedScopeReason>(Categories.Pats, Severities.Ok);
    }

    [Test]
    public void Audit_WhenScopesHasAppTokenScope_ReturnsPatHasAppScopeReason()
    {
        var fixture = CreateFixture();

        var auditee = fixture.Build<SessionTokenDto>()
            .With(x => x.Scope, "app_token")
            .Create();

        var sut = fixture.Create<PersonalAccessTokenScopesAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult<PatHasAppScopeReason>(Categories.Pats, Severities.Critical);
    }

    private static Fixture CreateFixture()
    {
        return new Fixture();
    }
}