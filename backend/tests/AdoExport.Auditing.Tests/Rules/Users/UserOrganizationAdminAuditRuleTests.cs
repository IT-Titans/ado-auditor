using AdoExport.Auditing.Rules.Users;
using AdoExport.Auditing.Rules.Users.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Tests.Rules.Users;

[TestFixture]
public class UserOrganizationAdminAuditRuleTests
{
    [Test]
    public void Audit_WhenAuditeeIsNull_Throws()
    {
        var fixture = CreateFixture();

        var sut = fixture.Create<UserOrganizationAdminAuditRule>();

        Assert.Throws<ArgumentNullException>(() => sut.Audit(null!));
    }

    [Test]
    public void Audit_WhenUserIsNotInAnyGroups_ReturnsUserIsNotOrganizationAdminReason()
    {
        var fixture = CreateFixture();

        var auditeeUserDescriptor = fixture.Create<string>();
        var auditee = fixture
            .Build<UserDto>()
            .OmitAutoProperties()
            .With(x => x.Descriptor, auditeeUserDescriptor)
            .Create();

        var rawAuditData = fixture
            .Build<RawAuditData>()
            .OmitAutoProperties()
            .With(x => x.UsersWithProjectEntitlements, [
                fixture
                    .Build<GetUserEntitlementsByUserEntitlementIdResponseDto>()
                    .OmitAutoProperties()
                    .With(x => x.User, auditee)
                    .Create(),
            ])
            .With(
                x => x.UserGroupMemberships,
                new Dictionary<string, GroupDto[]>()
                {
                    {
                        auditeeUserDescriptor, []
                    }
                })
            .Create();
        fixture.Inject(rawAuditData);

        var sut = fixture.Create<UserOrganizationAdminAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult<UserIsNotOrganizationAdminReason>(Categories.Users, Severities.Ok);
    }

    [Test]
    public void Audit_WhenUserIsNotInProjectCollectionAdminGroup_ReturnsUserIsNotOrganizationAdminReason()
    {
        var fixture = CreateFixture();

        var auditeeUserDescriptor = fixture.Create<string>();
        var auditee = fixture
            .Build<UserDto>()
            .OmitAutoProperties()
            .With(x => x.Descriptor, auditeeUserDescriptor)
            .Create();

        var rawAuditData = fixture
            .Build<RawAuditData>()
            .OmitAutoProperties()
            .With(x => x.UsersWithProjectEntitlements, [
                fixture
                    .Build<GetUserEntitlementsByUserEntitlementIdResponseDto>()
                    .OmitAutoProperties()
                    .With(x => x.User, auditee)
                    .Create(),
            ])
            .With(
                x => x.UserGroupMemberships,
                new Dictionary<string, GroupDto[]>()
                {
                    {
                        auditeeUserDescriptor, [
                            fixture
                                .Build<GroupDto>()
                                .OmitAutoProperties()
                                .With(x => x.PrincipalName, fixture.Create<string>())
                                .Create()
                        ]
                    }
                })
            .Create();
        fixture.Inject(rawAuditData);

        var sut = fixture.Create<UserOrganizationAdminAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult<UserIsNotOrganizationAdminReason>(Categories.Users, Severities.Ok);
    }

    [Test]
    public void Audit_WhenUserIsInProjectCollectionAdminGroup_ReturnsUserIsOrganizationAdminReason()
    {
        var fixture = CreateFixture();

        var auditeeUserDescriptor = fixture.Create<string>();
        var auditee = fixture
            .Build<UserDto>()
            .OmitAutoProperties()
            .With(x => x.Descriptor, auditeeUserDescriptor)
            .Create();

        var rawAuditData = fixture
            .Build<RawAuditData>()
            .OmitAutoProperties()
            .With(x => x.UsersWithProjectEntitlements, [
                fixture
                    .Build<GetUserEntitlementsByUserEntitlementIdResponseDto>()
                    .OmitAutoProperties()
                    .With(x => x.User, auditee)
                    .Create(),
            ])
            .With(
                x => x.UserGroupMemberships,
                new Dictionary<string, GroupDto[]>()
                {
                    {
                        auditeeUserDescriptor, [
                            fixture
                                .Build<GroupDto>()
                                .OmitAutoProperties()
                                .With(x => x.PrincipalName, "collection-id\\Project Collection Administrators")
                                .Create()
                        ]
                    }
                })
            .Create();
        fixture.Inject(rawAuditData);

        var sut = fixture.Create<UserOrganizationAdminAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult<UserIsOrganizationAdminReason>(Categories.Users, Severities.Warning);
    }

    [Test]
    public void Audit_ReturnsExpectedReasonForGivenNumberOfPATs()
    {
        var fixture = CreateFixture();

        var auditeeUserDescriptor = fixture.Create<string>();
        var auditee = fixture
            .Build<UserDto>()
            .OmitAutoProperties()
            .With(x => x.Descriptor, auditeeUserDescriptor)
            .Create();

        var rawAuditData = fixture
            .Build<RawAuditData>()
            .OmitAutoProperties()
            .With(x => x.UsersWithProjectEntitlements, [
                fixture
                    .Build<GetUserEntitlementsByUserEntitlementIdResponseDto>()
                    .OmitAutoProperties()
                    .With(x => x.User, auditee)
                    .Create(),
            ])
            .With(
                x => x.UserGroupMemberships,
                new Dictionary<string, GroupDto[]>()
                {
                    {
                        auditeeUserDescriptor, []
                    }
                })
            .Create();
        fixture.Inject(rawAuditData);

        var sut = fixture.Create<UserOrganizationAdminAuditRule>();

        var ruleResult = sut.Audit(auditee);

        ruleResult.AssertRuleResult<UserIsNotOrganizationAdminReason>(Categories.Users, Severities.Ok);
    }

    private static Fixture CreateFixture()
    {
        return new Fixture();
    }
}