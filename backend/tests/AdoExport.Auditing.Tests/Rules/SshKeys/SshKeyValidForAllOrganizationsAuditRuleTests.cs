using AdoExport.Auditing.Rules.SshKeys;
using AdoExport.Auditing.Rules.SshKeys.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Tests.Rules.SshKeys;

[TestFixture]
public class SshKeyValidForAllOrganizationsAuditRuleTests
{
    [Test]
    public void Audit_WhenAuditeeIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => CreateSut().Audit(null!));
    }

    [Test]
    public void Audit_WhenTargetAccountsIsNull_FlagsKeyAsValidForAllOrganizationsAsCritical()
    {
        var ruleResult = CreateSut().Audit(new SessionTokenDto { TargetAccounts = null });

        ruleResult.AssertRuleResult<SshKeyIsValidForAllOrganizationsReason>(Categories.Pats, Severities.Critical);
    }

    [Test]
    public void Audit_WhenTargetAccountsIsEmpty_FlagsKeyAsValidForAllOrganizationsAsCritical()
    {
        var ruleResult = CreateSut().Audit(new SessionTokenDto { TargetAccounts = [] });

        ruleResult.AssertRuleResult<SshKeyIsValidForAllOrganizationsReason>(Categories.Pats, Severities.Critical);
    }

    [Test]
    public void Audit_WhenKeyIsBoundToExactlyOneOrganization_IsOk()
    {
        var ruleResult = CreateSut().Audit(new SessionTokenDto { TargetAccounts = ["org-1"] });

        ruleResult.AssertRuleResult<SshKeyIsValidForOnlyOneOrganizationReason>(Categories.Pats, Severities.Ok);
    }

    [TestCase(2)]
    [TestCase(3)]
    [TestCase(50)]
    public void Audit_WhenKeyIsBoundToMoreThanOneOrganization_WarnsAboutIt(int organizations)
    {
        var accounts = Enumerable.Range(1, organizations).Select(i => $"org-{i}").ToList();

        var ruleResult = CreateSut().Audit(new SessionTokenDto { TargetAccounts = accounts });

        ruleResult.AssertRuleResult<SshKeyIsValidForMoreThanOneOrganizationReason>(Categories.Pats, Severities.Warning);
    }

    private static SshKeyValidForAllOrganizationsAuditRule CreateSut()
    {
        return new SshKeyValidForAllOrganizationsAuditRule(new Fixture().Create<RawAuditData>());
    }
}
