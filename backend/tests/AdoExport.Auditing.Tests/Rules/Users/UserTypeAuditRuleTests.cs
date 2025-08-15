using AdoExport.Auditing.Rules.Users;
using AdoExport.Auditing.Rules.Users.Reasons;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Tests.Rules.Users;

[TestFixture]
public class UserTypeAuditRuleTests
{
    [Test]
    public void Audit_WhenAuditeeIsNull_Throws()
    {
        var fixture = CreateFixture();

        var sut = fixture.Create<UserTypeAuditRule>();

        Assert.Throws<ArgumentNullException>(() => sut.Audit(null!));
    }

    [Test]
    public void Audit_WhenAuditeeIsMember_ReturnsUserIsOfTypeMemberReason()
    {
        var fixture = CreateFixture();

        var auditee = fixture
            .Build<UserDto>()
            .OmitAutoProperties()
            .With(x => x.MetaType, "member")
            .Create();

        var sut = fixture.Create<UserTypeAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult<UserIsOfTypeMemberReason>(Categories.Users, Severities.Ok);
    }

    [Test]
    public void Audit_WhenAuditeeIsGuest_ReturnsUserIsOfTypeGuestReason()
    {
        var fixture = CreateFixture();

        var auditee = fixture
            .Build<UserDto>()
            .OmitAutoProperties()
            .With(x => x.MetaType, "guest")
            .Create();

        var sut = fixture.Create<UserTypeAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult<UserIsOfTypeGuestReason>(Categories.Users, Severities.Warning);
    }

    [Test]
    public void Audit_WhenAuditeeIsBuildAgent_ReturnsUserIsOfTypeBuildAgentReason()
    {
        var fixture = CreateFixture();

        var auditee = fixture
            .Build<UserDto>()
            .OmitAutoProperties()
            .With(x => x.MetaType, (string?)null)
            .With(x => x.Domain, "build")
            .Create();

        var sut = fixture.Create<UserTypeAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult<UserIsOfTypeBuildAgentReason>(Categories.Users, Severities.Ok);
    }

    [Test]
    public void Audit_WhenAuditeeIsAgentPool_ReturnsUserIsOfTypeAgentPoolReason()
    {
        var fixture = CreateFixture();

        var auditee = fixture
            .Build<UserDto>()
            .OmitAutoProperties()
            .With(x => x.MetaType, (string?)null)
            .With(x => x.Domain, "agentpool")
            .Create();

        var sut = fixture.Create<UserTypeAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult<UserIsOfTypeAgentPoolReason>(Categories.Users, Severities.Ok);
    }

    private static Fixture CreateFixture()
    {
        return new Fixture();
    }
}