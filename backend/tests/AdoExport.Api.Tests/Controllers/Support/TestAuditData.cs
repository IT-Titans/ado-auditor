using AdoExport.Auditing;
using AdoExport.Auditing.Rules;
using AdoExport.Auditing.Rules.Pats.Reasons;
using AdoExport.Auditing.Rules.Users.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Api.Tests.Controllers.Support;

/// <summary>
/// A small but complete audit: two PATs owned by two users, one of them an organization
/// admin, plus the raw data the user repository reads next to the audit itself.
/// </summary>
/// <remarks>
/// Rule ids are the real ones from the reason classes, so the expectations below read as
/// "this rule is reported for these items", not as invented fixtures.
/// </remarks>
public sealed class TestAuditData
{
    /// <summary>PatHasAppScopeReason, critical, reported for both PATs.</summary>
    public static readonly Guid PatAppScopeRuleId = Guid.Parse("5a5adb71-e8ac-40d3-8956-15e6465be028");

    /// <summary>PatExpiresInOverOneQuarterReason, warning, reported for the first PAT only.</summary>
    public static readonly Guid PatExpiresOverQuarterRuleId = Guid.Parse("27bd636c-6a92-4c77-9023-d91d3a0dc129");

    /// <summary>UserIsOrganizationAdminReason, warning, reported for the first user only.</summary>
    public static readonly Guid UserIsAdminRuleId = Guid.Parse("d9883edd-8181-4acb-9e13-b44e04d2ca7d");

    public const string OrganizationName = "ITTitans";

    public const string FirstPatId = "pat-1";

    public const string SecondPatId = "pat-2";

    public const string FirstUserDescriptor = "aad.alice";

    public const string SecondUserDescriptor = "aad.bob";

    /// <summary>A user that exists in the organization but owns no PAT and has no access level.</summary>
    public const string UserWithoutAccessLevelDescriptor = "aad.carol";

    public required Audit Audit { get; init; }

    public required CreateAuditData CreateAuditData { get; init; }

    public static TestAuditData Create(Guid? auditId = null)
    {
        var metadata = new AuditMetadata()
        {
            AuditId = auditId ?? Guid.NewGuid(),
            OrganizationName = OrganizationName,
            ExpiresAfter = TimeSpan.FromMinutes(30),
            RequestedAuditAt = new DateTime(2026, 11, 12, 8, 0, 0, DateTimeKind.Utc),
            StartRequestAuditDataAt = new DateTime(2026, 11, 12, 8, 0, 1, DateTimeKind.Utc),
            FinishRequestAuditDataAt = new DateTime(2026, 11, 12, 8, 0, 5, DateTimeKind.Utc),
            CreatedAuditAt = new DateTime(2026, 11, 12, 8, 0, 9, DateTimeKind.Utc),
        };

        var alice = CreateUser(FirstUserDescriptor, "Alice", "alice-id");
        var bob = CreateUser(SecondUserDescriptor, "Bob", "bob-id");
        var carol = CreateUser(UserWithoutAccessLevelDescriptor, "Carol", "carol-id");

        var firstPat = CreatePat(FirstPatId, "Alice PAT", alice.OriginId);
        var secondPat = CreatePat(SecondPatId, "Bob PAT", bob.OriginId);

        var audit = new Audit(metadata)
        {
            PatAuditResults =
            [
                new AuditResult<SessionTokenDto>()
                {
                    Auditee = firstPat,
                    RuleResults =
                    [
                        Result(new PatHasAppScopeReason(firstPat)),
                        Result(new PatExpiresInOverOneQuarterReason(firstPat)),
                    ],
                },
                new AuditResult<SessionTokenDto>()
                {
                    Auditee = secondPat,
                    RuleResults = [Result(new PatHasAppScopeReason(secondPat))],
                },
            ],
            SshKeysAuditResults = [],
            UserAuditResults =
            [
                new AuditResult<UserDto>()
                {
                    Auditee = alice,
                    RuleResults = [Result(new UserIsOrganizationAdminReason(alice))],
                },
                new AuditResult<UserDto>()
                {
                    Auditee = bob,
                    RuleResults = [Result(new UserIsNotOrganizationAdminReason(bob))],
                },
            ],
        };

        var createAuditData = new CreateAuditData()
        {
            Metadata = metadata,
            ExportedData = new RawAuditData()
            {
                UserGroupMemberships = new Dictionary<string, GroupDto[]>(),
                AllUsers = [alice, bob, carol],
                UsersWithProjectEntitlements =
                [
                    CreateEntitlement(alice, "active"),
                    CreateEntitlement(bob, "disabled"),
                ],
                PrivateSessionTokenDetailsWithUser =
                [
                    new SessionTokenWithUser() { SessionToken = firstPat, User = alice },
                    new SessionTokenWithUser() { SessionToken = secondPat, User = bob },
                ],
                PublicSessionTokenDetailsWithUser = [],
            },
        };

        return new TestAuditData()
        {
            Audit = audit,
            CreateAuditData = createAuditData,
        };
    }

    private static RuleResult Result(AuditRuleReason reason) => new() { Reason = reason };

    private static UserDto CreateUser(string descriptor, string displayName, string originId) => new()
    {
        Descriptor = descriptor,
        DisplayName = displayName,
        OriginId = originId,
        MetaType = UserDto.MemberUserMetaType,
    };

    private static SessionTokenDto CreatePat(string authorizationId, string displayName, string userId) => new()
    {
        AuthorizationId = authorizationId,
        DisplayName = displayName,
        UserId = userId,
        ValidFrom = "2026-10-01T00:00:00Z",
        ValidTo = "2027-03-01T00:00:00Z",
        Scope = "vso.code vso.build",
    };

    private static GetUserEntitlementsByUserEntitlementIdResponseDto CreateEntitlement(UserDto user, string status) => new()
    {
        User = user,
        Id = Guid.NewGuid().ToString(),
        AccessLevel = new AccessLevelDto() { Status = status },
        ProjectEntitlements = [],
    };
}
