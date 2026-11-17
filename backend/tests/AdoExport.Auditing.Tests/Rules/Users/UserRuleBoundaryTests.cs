using AdoExport.Auditing.Rules.Users;
using AdoExport.Auditing.Rules.Users.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Tests.Rules.Users;

/// <summary>
/// Boundary and missing-data cases of the user rules that the happy-path tests next to this file do not reach.
/// </summary>
[TestFixture]
public class UserRuleBoundaryTests
{
    // UserTypeAuditRule

    [TestCase("MEMBER")]
    [TestCase("Member")]
    public void UserType_MetaTypeIsMatchedCaseInsensitively_Member(string metaType)
    {
        var result = new UserTypeAuditRule(EmptyData()).Audit(new UserDto { MetaType = metaType });

        result.AssertRuleResult<UserIsOfTypeMemberReason>(Categories.Users, Severities.Ok);
    }

    [TestCase("GUEST")]
    [TestCase("Guest")]
    public void UserType_MetaTypeIsMatchedCaseInsensitively_Guest(string metaType)
    {
        var result = new UserTypeAuditRule(EmptyData()).Audit(new UserDto { MetaType = metaType });

        result.AssertRuleResult<UserIsOfTypeGuestReason>(Categories.Users, Severities.Warning);
    }

    [TestCase("BUILD")]
    [TestCase("Build")]
    public void UserType_BuildDomainIsMatchedCaseInsensitively(string domain)
    {
        var result = new UserTypeAuditRule(EmptyData()).Audit(new UserDto { MetaType = null, Domain = domain });

        result.AssertRuleResult<UserIsOfTypeBuildAgentReason>(Categories.Users, Severities.Ok);
    }

    [Test]
    public void UserType_AgentPoolDomainWithoutMetaType_IsAgentPool()
    {
        var result = new UserTypeAuditRule(EmptyData()).Audit(new UserDto { MetaType = null, Domain = "AgentPool" });

        result.AssertRuleResult<UserIsOfTypeAgentPoolReason>(Categories.Users, Severities.Ok);
    }

    [Test]
    public void UserType_BuildDomainWithMemberMetaType_IsAMemberNotABuildAgent()
    {
        var result = new UserTypeAuditRule(EmptyData()).Audit(new UserDto { MetaType = "member", Domain = "build" });

        result.AssertRuleResult<UserIsOfTypeMemberReason>(Categories.Users, Severities.Ok);
    }

    [Test]
    public void UserType_GuestMetaTypeWinsOverBuildDomain()
    {
        var result = new UserTypeAuditRule(EmptyData()).Audit(new UserDto { MetaType = "guest", Domain = "build" });

        result.AssertRuleResult<UserIsOfTypeGuestReason>(Categories.Users, Severities.Warning);
    }

    [Test]
    public void UserType_WithoutMetaTypeAndWithoutDomain_IsUnknownAndCritical()
    {
        var result = new UserTypeAuditRule(EmptyData()).Audit(new UserDto { MetaType = null, Domain = null });

        result.AssertRuleResult<UserIsOfTypeUnknownReason>(Categories.Users, Severities.Critical);
    }

    [Test]
    public void UserType_WithoutMetaTypeAndWithForeignDomain_IsUnknownAndCritical()
    {
        var result = new UserTypeAuditRule(EmptyData()).Audit(new UserDto { MetaType = null, Domain = "contoso" });

        result.AssertRuleResult<UserIsOfTypeUnknownReason>(Categories.Users, Severities.Critical);
    }

    [Test]
    public void UserType_WithUnrecognisedMetaType_IsUnknownAndCritical()
    {
        var result = new UserTypeAuditRule(EmptyData()).Audit(new UserDto { MetaType = "service", Domain = "build" });

        result.AssertRuleResult<UserIsOfTypeUnknownReason>(Categories.Users, Severities.Critical);
    }

    // UserPatsAuditRule

    [Test]
    public void UserPats_DescriptorsAreComparedCaseInsensitively()
    {
        var owner = new UserDto { Descriptor = "aad.ABC" };
        var tokens = Enumerable.Range(0, 4)
            .Select(_ => new SessionTokenWithUser { SessionToken = new SessionTokenDto(), User = new UserDto { Descriptor = "AAD.abc" } })
            .ToArray();

        var result = new UserPatsAuditRule(EmptyData(privateTokens: tokens)).Audit(owner);

        result.AssertRuleResult<UserExceedsOverallPatLimitReason>(Categories.Users, Severities.Warning);
    }

    [Test]
    public void UserPats_TokensOfOtherUsersAreNotCounted()
    {
        var owner = new UserDto { Descriptor = "owner" };
        var tokens = Enumerable.Range(0, 10)
            .Select(i => new SessionTokenWithUser { SessionToken = new SessionTokenDto(), User = new UserDto { Descriptor = $"other-{i}" } })
            .ToArray();

        var result = new UserPatsAuditRule(EmptyData(privateTokens: tokens)).Audit(owner);

        result.AssertRuleResult<UserIsWithinOverallPatLimitReason>(Categories.Users, Severities.Ok);
    }

    [Test]
    public void UserPats_TokensWithoutResolvedUserAreIgnoredInsteadOfFailingTheAudit()
    {
        var owner = new UserDto { Descriptor = "owner" };
        var tokens = Enumerable.Range(0, 5)
            .Select(_ => new SessionTokenWithUser { SessionToken = new SessionTokenDto(), User = null! })
            .ToArray();

        var result = new UserPatsAuditRule(EmptyData(privateTokens: tokens)).Audit(owner);

        result.AssertRuleResult<UserIsWithinOverallPatLimitReason>(Categories.Users, Severities.Ok);
    }

    [Test]
    public void UserPats_SshKeysDoNotCountTowardsThePatLimit()
    {
        var owner = new UserDto { Descriptor = "owner" };
        var keys = Enumerable.Range(0, 10)
            .Select(_ => new SessionTokenWithUser { SessionToken = new SessionTokenDto(), User = owner })
            .ToArray();

        var result = new UserPatsAuditRule(EmptyData(publicTokens: keys)).Audit(owner);

        result.AssertRuleResult<UserIsWithinOverallPatLimitReason>(Categories.Users, Severities.Ok);
    }

    // UserOrganizationAdminAuditRule

    [TestCase("[contoso]\\Project Collection Administrators")]
    [TestCase("vstfs:///Framework/IdentityDomain/abc\\Project Collection Administrators")]
    public void UserAdmin_AnyCollectionAdministratorsGroupMembershipMakesTheUserAdmin(string principalName)
    {
        var result = AuditAdmin(new GroupDto { PrincipalName = principalName });

        result.AssertRuleResult<UserIsOrganizationAdminReason>(Categories.Users, Severities.Warning);
    }

    [TestCase("[contoso]\\Project Collection Administrators Extra")]
    [TestCase("[contoso]\\Project Collection Service Accounts")]
    [TestCase("[contoso]\\Contributors")]
    [TestCase("Project Collection Administrators")]
    public void UserAdmin_OtherGroupsDoNotMakeTheUserAdmin(string principalName)
    {
        var result = AuditAdmin(new GroupDto { PrincipalName = principalName });

        result.AssertRuleResult<UserIsNotOrganizationAdminReason>(Categories.Users, Severities.Ok);
    }

    [Test]
    public void UserAdmin_OneAdministratorGroupAmongSeveralGroupsIsEnough()
    {
        var result = AuditAdmin(
            new GroupDto { PrincipalName = "[contoso]\\Contributors" },
            new GroupDto { PrincipalName = "[contoso]\\Project Collection Administrators" },
            new GroupDto { PrincipalName = "[contoso]\\Readers" });

        result.AssertRuleResult<UserIsOrganizationAdminReason>(Categories.Users, Severities.Warning);
    }

    [Test]
    public void UserAdmin_AnotherUsersAdminMembershipDoesNotMakeTheAuditeeAdmin()
    {
        var auditee = new UserDto { Descriptor = "auditee" };
        var admin = new UserDto { Descriptor = "admin" };
        var data = new RawAuditData
        {
            AllUsers = [],
            PrivateSessionTokenDetailsWithUser = [],
            PublicSessionTokenDetailsWithUser = [],
            UsersWithProjectEntitlements =
            [
                new GetUserEntitlementsByUserEntitlementIdResponseDto { User = auditee },
                new GetUserEntitlementsByUserEntitlementIdResponseDto { User = admin },
            ],
            UserGroupMemberships = new Dictionary<string, GroupDto[]>
            {
                ["admin"] = [new GroupDto { PrincipalName = "[contoso]\\Project Collection Administrators" }],
                ["auditee"] = [],
            },
        };

        var result = new UserOrganizationAdminAuditRule(data).Audit(auditee);

        result.AssertRuleResult<UserIsNotOrganizationAdminReason>(Categories.Users, Severities.Ok);
    }

    [Test]
    public void UserAdmin_WhenUserHasNoMembershipEntry_IsNotAdmin()
    {
        var auditee = new UserDto { Descriptor = "auditee" };
        var data = new RawAuditData
        {
            AllUsers = [],
            PrivateSessionTokenDetailsWithUser = [],
            PublicSessionTokenDetailsWithUser = [],
            UsersWithProjectEntitlements = [new GetUserEntitlementsByUserEntitlementIdResponseDto { User = auditee }],
            UserGroupMemberships = new Dictionary<string, GroupDto[]>(),
        };

        var result = new UserOrganizationAdminAuditRule(data).Audit(auditee);

        result.AssertRuleResult<UserIsNotOrganizationAdminReason>(Categories.Users, Severities.Ok);
    }

    [Test]
    public void UserAdmin_WithoutAnyEntitlements_IsNotAdmin()
    {
        var result = new UserOrganizationAdminAuditRule(EmptyData()).Audit(new UserDto { Descriptor = "auditee" });

        result.AssertRuleResult<UserIsNotOrganizationAdminReason>(Categories.Users, Severities.Ok);
    }

    private static AdoExport.Auditing.Rules.RuleResult AuditAdmin(params GroupDto[] groups)
    {
        var auditee = new UserDto { Descriptor = "auditee" };
        var data = new RawAuditData
        {
            AllUsers = [],
            PrivateSessionTokenDetailsWithUser = [],
            PublicSessionTokenDetailsWithUser = [],
            UsersWithProjectEntitlements = [new GetUserEntitlementsByUserEntitlementIdResponseDto { User = auditee }],
            UserGroupMemberships = new Dictionary<string, GroupDto[]> { ["auditee"] = groups },
        };

        return new UserOrganizationAdminAuditRule(data).Audit(auditee);
    }

    private static RawAuditData EmptyData(SessionTokenWithUser[]? privateTokens = null, SessionTokenWithUser[]? publicTokens = null)
    {
        return new RawAuditData
        {
            AllUsers = [],
            UserGroupMemberships = new Dictionary<string, GroupDto[]>(),
            UsersWithProjectEntitlements = [],
            PrivateSessionTokenDetailsWithUser = privateTokens ?? [],
            PublicSessionTokenDetailsWithUser = publicTokens ?? [],
        };
    }
}
