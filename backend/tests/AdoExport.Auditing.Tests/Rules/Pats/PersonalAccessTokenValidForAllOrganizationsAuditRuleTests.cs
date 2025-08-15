using AdoExport.Auditing.Rules.Pats;
using AdoExport.Auditing.Rules.Pats.Reasons;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Tests.Rules.Pats;

[TestFixture]
public class PersonalAccessTokenValidForAllOrganizationsAuditRuleTests
{
    [Test]
    public void Audit_WhenAuditeeIsNull_Throws()
    {
        var fixture = CreateFixture();

        var sut = fixture.Create<PersonalAccessTokenValidForAllOrganizationsAuditRule>();

        Assert.Throws<ArgumentNullException>(() =>  sut.Audit(null!));
    }

    [Test]
    public void Audit_WhenTargetAccountsHasSingleEntry_ReturnsPatIsValidForOnlyOneOrganizationReason()
    {
        var fixture = CreateFixture();

        var auditee = fixture.Build<SessionTokenDto>()
            .With(x => x.TargetAccounts, [fixture.Build<string>().Create()])
            .Create();

        var sut = fixture.Create<PersonalAccessTokenValidForAllOrganizationsAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult<PatIsValidForOnlyOneOrganizationReason>(Categories.Pats, Severities.Ok);
    }

    [Test]
    public void Audit_WhenTargetAccountsHasMultipleEntries_ReturnsPatIsValidForMoreThanOneOrganizationReason()
    {
        var fixture = CreateFixture();

        var auditee = fixture.Build<SessionTokenDto>()
            .With(x => x.TargetAccounts, [..fixture.Build<string>().CreateMany(2)])
            .Create();

        var sut = fixture.Create<PersonalAccessTokenValidForAllOrganizationsAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult<PatIsValidForMoreThanOneOrganizationReason>(Categories.Pats, Severities.Warning);
    }

    private static IEnumerable<TestCaseData> PatIsValidForAllOrganizationsTestCases()
    {
        var fixture = CreateFixture();

        yield return new TestCaseData(
            fixture.Build<SessionTokenDto>()
                .With(x => x.TargetAccounts, (List<string>?)null)
                .Create());

        yield return new TestCaseData(
            fixture.Build<SessionTokenDto>()
                .With(x => x.TargetAccounts, [])
                .Create());
    }

    [TestCaseSource(nameof(PatIsValidForAllOrganizationsTestCases))]
    public void Audit_WhenTargetAccountsIsNullOrEmpty_ReturnsPatIsValidForAllOrganizationsReason(SessionTokenDto auditee)
    {
        var fixture = CreateFixture();

        var sut = fixture.Create<PersonalAccessTokenValidForAllOrganizationsAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult<PatIsValidForAllOrganizationsReason>(Categories.Pats, Severities.Critical);
    }

    private static Fixture CreateFixture()
    {
        return new Fixture();
    }
}