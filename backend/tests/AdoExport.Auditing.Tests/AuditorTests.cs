using AdoExport.Auditing.Rules.Pats.Reasons;
using AdoExport.Auditing.Rules.SshKeys.Reasons;
using AdoExport.Auditing.Rules.Users.Reasons;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;
using AdoExport.Common.Services;

namespace AdoExport.Auditing.Tests;

[TestFixture]
public class AuditorTests
{
    private static readonly DateTime StartedAt = new(2026, 11, 12, 8, 0, 0, DateTimeKind.Utc);

    private static readonly DateTime FinishedAt = new(2026, 11, 12, 8, 0, 5, DateTimeKind.Utc);

    [Test]
    public void Constructor_WhenDateTimeServiceIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new Auditor(null!));
    }

    [Test]
    public void AuditOrganizationAsync_WhenCreateAuditDataIsNull_Throws()
    {
        var sut = CreateSut();

        Assert.ThrowsAsync<ArgumentNullException>(() => sut.AuditOrganizationAsync(null!));
    }

    [Test]
    public async Task AuditOrganizationAsync_WithEmptyData_ReturnsSuccessfulAuditWithoutFindings()
    {
        var sut = CreateSut();
        var data = CreateAuditData(CreateRawData());

        var result = await sut.AuditOrganizationAsync(data);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatAuditResults.Should().BeEmpty();
        result.Value.SshKeysAuditResults.Should().BeEmpty();
        result.Value.UserAuditResults.Should().BeEmpty();
    }

    [Test]
    public async Task AuditOrganizationAsync_CarriesMetadataIntoTheAudit()
    {
        var sut = CreateSut();
        var data = CreateAuditData(CreateRawData());

        var result = await sut.AuditOrganizationAsync(data);

        result.Value.Metadata.Should().BeSameAs(data.Metadata);
        result.Value.AuditId.Should().Be(data.Metadata.AuditId);
        result.Value.OrganizationName.Should().Be("contoso");
    }

    [Test]
    public async Task AuditOrganizationAsync_RecordsStartAndFinishOfTheCalculation()
    {
        var sut = CreateSut();
        var data = CreateAuditData(CreateRawData());

        await sut.AuditOrganizationAsync(data);

        data.Metadata.StartCalculatingAuditAt.Should().Be(StartedAt);
        data.Metadata.FinishCalculatingAuditAt.Should().Be(FinishedAt);
    }

    [Test]
    public async Task AuditOrganizationAsync_AuditsEveryPrivateTokenAsPatWithAllThreePatRules()
    {
        var sut = CreateSut();
        var user = Member("user-1");
        var data = CreateAuditData(CreateRawData(
            privateTokens:
            [
                new SessionTokenWithUser { SessionToken = Token(365, scope: "app_token"), User = user },
                new SessionTokenWithUser { SessionToken = Token(30, scope: "vso.code", accounts: ["org"]), User = user },
            ]));

        var result = await sut.AuditOrganizationAsync(data);

        var pats = result.Value.PatAuditResults;
        pats.Should().HaveCount(2);
        pats[0].RuleResults.Select(r => r.Reason.GetType()).Should().Equal(
            typeof(PatExpiresInOverHalfAYearReason),
            typeof(PatHasAppScopeReason),
            typeof(PatIsValidForAllOrganizationsReason));
        pats[1].RuleResults.Select(r => r.Reason.GetType()).Should().Equal(
            typeof(PatExpiresWithinShortTimeRangeReason),
            typeof(PatHasLimitedScopeReason),
            typeof(PatIsValidForOnlyOneOrganizationReason));
        pats.GetTotalRuleResultsBySeverity(Severities.Critical).Should().Be(3);
    }

    [Test]
    public async Task AuditOrganizationAsync_AuditsEveryPublicTokenAsSshKeyWithBothSshKeyRules()
    {
        var sut = CreateSut();
        var user = Member("user-1");
        var data = CreateAuditData(CreateRawData(
            publicTokens: [new SessionTokenWithUser { SessionToken = Token(400, accounts: ["a", "b"]), User = user }]));

        var result = await sut.AuditOrganizationAsync(data);

        var keys = result.Value.SshKeysAuditResults;
        keys.Should().ContainSingle();
        keys[0].RuleResults.Select(r => r.Reason.GetType()).Should().Equal(
            typeof(SshKeyExpiresInOverAYearReason),
            typeof(SshKeyIsValidForMoreThanOneOrganizationReason));
    }

    [Test]
    public async Task AuditOrganizationAsync_KeepsPatsAndSshKeysApart()
    {
        var sut = CreateSut();
        var user = Member("user-1");
        var privateToken = Token(10);
        var publicToken = Token(10);
        var data = CreateAuditData(CreateRawData(
            privateTokens: [new SessionTokenWithUser { SessionToken = privateToken, User = user }],
            publicTokens: [new SessionTokenWithUser { SessionToken = publicToken, User = user }]));

        var result = await sut.AuditOrganizationAsync(data);

        result.Value.PatAuditResults.Single().Auditee.Should().BeSameAs(privateToken);
        result.Value.SshKeysAuditResults.Single().Auditee.Should().BeSameAs(publicToken);
    }

    [Test]
    public async Task AuditOrganizationAsync_AuditsMembersAndGuestsButSkipsBuildAndAgentPoolIdentities()
    {
        var sut = CreateSut();
        var member = Member("member");
        var guest = new UserDto { Descriptor = "guest", MetaType = UserDto.GuestUserMetaType };
        var buildIdentity = new UserDto { Descriptor = "build", MetaType = null, Domain = UserDto.BuildUserDomain };
        var agentPool = new UserDto { Descriptor = "pool", MetaType = null, Domain = UserDto.AgentPoolUserDomain };
        var unknownMetaType = new UserDto { Descriptor = "other", MetaType = "service" };
        var data = CreateAuditData(CreateRawData(users: [member, buildIdentity, guest, agentPool, unknownMetaType]));

        var result = await sut.AuditOrganizationAsync(data);

        result.Value.UserAuditResults.Select(r => r.Auditee).Should().Equal(member, guest);
    }

    [Test]
    public async Task AuditOrganizationAsync_AppliesTypePatLimitAndAdminRulesToEachUser()
    {
        var sut = CreateSut();
        var member = Member("member");
        var guest = new UserDto { Descriptor = "guest", MetaType = UserDto.GuestUserMetaType };
        var tokens = Enumerable.Range(0, 4)
            .Select(_ => new SessionTokenWithUser { SessionToken = Token(10), User = member })
            .ToArray();
        var data = CreateAuditData(CreateRawData(
            users: [member, guest],
            privateTokens: tokens,
            entitlements: [new GetUserEntitlementsByUserEntitlementIdResponseDto { User = member }],
            memberships: new Dictionary<string, GroupDto[]>
            {
                ["member"] = [new GroupDto { PrincipalName = "[contoso]\\Project Collection Administrators" }],
            }));

        var result = await sut.AuditOrganizationAsync(data);

        var results = result.Value.UserAuditResults;
        results[0].RuleResults.Select(r => r.Reason.GetType()).Should().Equal(
            typeof(UserIsOfTypeMemberReason),
            typeof(UserExceedsOverallPatLimitReason),
            typeof(UserIsOrganizationAdminReason));
        results[1].RuleResults.Select(r => r.Reason.GetType()).Should().Equal(
            typeof(UserIsOfTypeGuestReason),
            typeof(UserIsWithinOverallPatLimitReason),
            typeof(UserIsNotOrganizationAdminReason));
    }

    [Test]
    public void AuditOrganizationAsync_WhenExportedDataIsMissing_ThrowsInsteadOfReportingAllClear()
    {
        var sut = CreateSut();
        var data = CreateAuditData(null);

        Assert.ThrowsAsync<ArgumentNullException>(() => sut.AuditOrganizationAsync(data));
    }

    private static Auditor CreateSut()
    {
        var dateTimeService = new Mock<IDateTimeService>();
        dateTimeService.SetupSequence(service => service.UtcNow())
            .Returns(StartedAt)
            .Returns(FinishedAt);

        return new Auditor(dateTimeService.Object);
    }

    private static CreateAuditData CreateAuditData(RawAuditData? rawData)
    {
        return new CreateAuditData
        {
            Metadata = new AuditMetadata
            {
                AuditId = Guid.NewGuid(),
                OrganizationName = "contoso",
                ExpiresAfter = TimeSpan.FromHours(1),
            },
            ExportedData = rawData,
        };
    }

    private static RawAuditData CreateRawData(
        UserDto[]? users = null,
        SessionTokenWithUser[]? privateTokens = null,
        SessionTokenWithUser[]? publicTokens = null,
        GetUserEntitlementsByUserEntitlementIdResponseDto[]? entitlements = null,
        Dictionary<string, GroupDto[]>? memberships = null)
    {
        return new RawAuditData
        {
            AllUsers = users ?? [],
            PrivateSessionTokenDetailsWithUser = privateTokens ?? [],
            PublicSessionTokenDetailsWithUser = publicTokens ?? [],
            UsersWithProjectEntitlements = entitlements ?? [],
            UserGroupMemberships = memberships ?? new Dictionary<string, GroupDto[]>(),
        };
    }

    private static UserDto Member(string descriptor)
    {
        return new UserDto { Descriptor = descriptor, MetaType = UserDto.MemberUserMetaType };
    }

    private static SessionTokenDto Token(int daysValid, string scope = "vso.code", List<string>? accounts = null)
    {
        var from = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return new SessionTokenDto
        {
            ValidFrom = from.ToString("o"),
            ValidTo = from.AddDays(daysValid).ToString("o"),
            Scope = scope,
            TargetAccounts = accounts,
        };
    }
}
