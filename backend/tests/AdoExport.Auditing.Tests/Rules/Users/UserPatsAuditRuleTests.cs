using AdoExport.Auditing.Rules.Users;
using AdoExport.Auditing.Rules.Users.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Tests.Rules.Users;

[TestFixture]
public class UserPatsAuditRuleTests
{
    [Test]
    public void Audit_WhenAuditeeIsNull_Throws()
    {
        var fixture = CreateFixture();

        var sut = fixture.Create<UserPatsAuditRule>();

        Assert.Throws<ArgumentNullException>(() => sut.Audit(null!));
    }

    // ReSharper disable once InconsistentNaming
    private static IEnumerable<TestCaseData> NumberOfPATsTestCases()
    {
        yield return new TestCaseData(typeof(UserIsWithinOverallPatLimitReason), 0, Severities.Ok);
        yield return new TestCaseData(typeof(UserIsWithinOverallPatLimitReason), 1, Severities.Ok);
        yield return new TestCaseData(typeof(UserIsWithinOverallPatLimitReason), 2, Severities.Ok);
        yield return new TestCaseData(typeof(UserIsWithinOverallPatLimitReason), 3, Severities.Ok);
        
        yield return new TestCaseData(typeof(UserExceedsOverallPatLimitReason), 4, Severities.Warning);
        yield return new TestCaseData(typeof(UserExceedsOverallPatLimitReason), 5, Severities.Warning);
    }

    [TestCaseSource(nameof(NumberOfPATsTestCases))]
    public void Audit_ReturnsExpectedReasonForGivenNumberOfPATs(Type expectedReasonType, int numberOfPats, Severities expectedSeverity)
    {
        var fixture = CreateFixture();

        var auditeeUserDescriptor = fixture.Create<string>();
        var auditee = fixture
            .Build<UserDto>()
            .OmitAutoProperties()
            .With(x => x.Descriptor, auditeeUserDescriptor)
            .Create();

        // generate required amount of PATs for the auditee
        var auditeePats = Enumerable
            .Range(0, numberOfPats)
            .Select(_ => CreatePrivateSessionTokenWithUser(fixture, auditeeUserDescriptor))
            .ToArray();

        var rawAuditData = fixture
            .Build<RawAuditData>()
            .OmitAutoProperties()
            .With(x => x.PrivateSessionTokenDetailsWithUser, [
                CreatePrivateSessionTokenWithUser(fixture, fixture.Create<string>()), // add PAT not related to our auditee
                ..auditeePats,
                CreatePrivateSessionTokenWithUser(fixture, fixture.Create<string>()) // add PAT not related to our auditee
            ])
            .Create();
        fixture.Inject(rawAuditData);

        var sut = fixture.Create<UserPatsAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult(expectedReasonType, Categories.Users, expectedSeverity);
    }

    private static SessionTokenWithUser CreatePrivateSessionTokenWithUser(Fixture fixture, string auditeeUserDescriptor)
    {
        return fixture
            .Build<SessionTokenWithUser>()
            .OmitAutoProperties()
            .With(
                x => x.SessionToken, 
                fixture
                    .Build<SessionTokenDto>()
                    .OmitAutoProperties()
                    .With(x => x.IsPublic, false)
                    .Create())
            .With(
                x => x.User,
                fixture
                    .Build<UserDto>()
                    .OmitAutoProperties()
                    .With(x => x.Descriptor, auditeeUserDescriptor)
                    .Create())
            .Create();
    }

    private static Fixture CreateFixture()
    {
        return new Fixture();
    }
}