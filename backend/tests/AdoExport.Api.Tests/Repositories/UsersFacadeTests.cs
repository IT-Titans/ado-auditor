using System.Collections.Immutable;

using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Errors;
using AdoExport.Api.Repositories;
using AdoExport.Api.Services;
using AdoExport.Auditing;
using AdoExport.Auditing.Rules;
using AdoExport.Client.AdoApi.Models;

using FluentResults;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using static AdoExport.Api.Tests.Repositories.RepositoryTestData;

using DomainCategories = AdoExport.Auditing.Categories;
using DomainSeverities = AdoExport.Auditing.Severities;

namespace AdoExport.Api.Tests.Repositories;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class UsersFacadeTests
{
    private readonly Mock<IAuditRepository> _auditRepository = new();

    private readonly Mock<IUserRepository> _userRepository = new();

    private readonly Mock<IRulesRepository> _rulesRepository = new();

    private readonly ScopedAuditKeyBearer _bearer = new();

    private UsersFacade CreateSut(ILogger<UsersFacade>? logger = null)
    {
        _rulesRepository
            .Setup(rules => rules.GetRuleDtosForAsync(It.IsAny<ImmutableArray<RuleResult>>()))
            .ReturnsAsync([]);

        return new UsersFacade(
            logger ?? NullLogger<UsersFacade>.Instance,
            _auditRepository.Object,
            _userRepository.Object,
            _rulesRepository.Object,
            _bearer);
    }

    private Task<T> Run<T>(Func<Task<T>> action) => InScopeAsync(_bearer, action);

    private void GivenAudit(Audit audit)
        => _auditRepository.Setup(repository => repository.GetAuditByIdAsync(AuditKey)).ReturnsAsync(Result.Ok(audit));

    private void GivenUsers(params UserDto[] users)
    {
        _userRepository.Setup(repository => repository.GetAllUsersAsync()).ReturnsAsync(users);
        foreach (var user in users)
        {
            _userRepository.Setup(repository => repository.GetUserByDescriptorAsync(user.Descriptor)).ReturnsAsync(user);
        }
    }

    private void GivenAccessLevel(UserDto user, string? status)
        => _userRepository
            .Setup(repository => repository.GetAccessLevelByUserDescriptorAsync(user.Descriptor))
            .ReturnsAsync(status is null ? null : AccessLevel(status));

    private void GivenTokenOwner(string userId, UserDto? owner)
        => _userRepository.Setup(repository => repository.GetUserByUserIdAsync(userId)).ReturnsAsync(owner);

    // ---- GetUserEntriesAsync ----

    [Test]
    public async Task GetUserEntries_PropagatesAFailedAudit()
    {
        _auditRepository
            .Setup(repository => repository.GetAuditByIdAsync(AuditKey))
            .ReturnsAsync(Result.Fail<Audit>(new AuditExpiredError(AuditKey)));
        var sut = CreateSut();

        var result = await Run(sut.GetUserEntriesAsync);

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Should().BeOfType<AuditExpiredError>();
        _userRepository.Verify(repository => repository.GetAllUsersAsync(), Times.Never);
    }

    [Test]
    public async Task GetUserEntries_ReturnsNoEntriesForAnOrganizationWithoutUsers()
    {
        GivenAudit(Audit());
        GivenUsers();
        var sut = CreateSut();

        var result = await Run(sut.GetUserEntriesAsync);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserEntries.Should().BeEmpty();
    }

    [Test]
    public async Task GetUserEntries_ListsOnlyMembersAndGuests()
    {
        var member = User("aad.member", "Member", "member");
        var guest = User("aad.guest", "Guest", "guest");
        var buildAgent = User("build.agent", "Build Service", "service");
        var untyped = User("aad.untyped", "Untyped", metaType: null);
        GivenAudit(Audit());
        GivenUsers(member, guest, buildAgent, untyped);
        foreach (var user in new[] { member, guest, buildAgent, untyped })
        {
            GivenAccessLevel(user, "active");
        }

        var sut = CreateSut();

        var result = await Run(sut.GetUserEntriesAsync);

        result.Value.UserEntries.Select(entry => entry.Id).Should().BeEquivalentTo(["aad.member", "aad.guest"]);
    }

    [Test]
    public async Task GetUserEntries_SkipsUsersWithoutAnAccessLevelButKeepsTheOthers()
    {
        var licensed = User("aad.licensed");
        var unlicensed = User("aad.unlicensed");
        GivenAudit(Audit());
        GivenUsers(unlicensed, licensed);
        GivenAccessLevel(licensed, "active");
        GivenAccessLevel(unlicensed, null);
        var sut = CreateSut();

        var result = await Run(sut.GetUserEntriesAsync);

        result.Value.UserEntries.Should().ContainSingle().Which.Id.Should().Be("aad.licensed");
    }

    [Test]
    public async Task GetUserEntries_MapsNameMetaTypeAndActiveFlag()
    {
        var active = User("aad.active", "Active Annie", "member");
        var disabled = User("aad.disabled", "Disabled Dan", "guest");
        GivenAudit(Audit());
        GivenUsers(active, disabled);
        GivenAccessLevel(active, "active");
        GivenAccessLevel(disabled, "disabled");
        var sut = CreateSut();

        var result = await Run(sut.GetUserEntriesAsync);

        var entries = result.Value.UserEntries.ToDictionary(entry => entry.Id);
        entries["aad.active"].UserName.Should().Be("Active Annie");
        entries["aad.active"].MetaType.Should().Be("member");
        entries["aad.active"].IsActive.Should().BeTrue();
        entries["aad.disabled"].UserName.Should().Be("Disabled Dan");
        entries["aad.disabled"].MetaType.Should().Be("guest");
        entries["aad.disabled"].IsActive.Should().BeFalse();
    }

    [Test]
    public async Task GetUserEntries_CountsRuleResultsPerSeverityOnlyForTheUserTheyBelongTo()
    {
        var anna = User("aad.Anna");
        var ben = User("aad.ben");
        var audit = Audit(users:
        [
            ResultFor(User("AAD.ANNA"), DomainSeverities.Critical, DomainSeverities.Critical, DomainSeverities.Warning),
            ResultFor(User("aad.anna"), DomainSeverities.Ok),
            ResultFor(User("aad.ben"), DomainSeverities.Warning, DomainSeverities.Ok, DomainSeverities.Ok),
        ]);
        GivenAudit(audit);
        GivenUsers(anna, ben);
        GivenAccessLevel(anna, "active");
        GivenAccessLevel(ben, "active");
        var sut = CreateSut();

        var result = await Run(sut.GetUserEntriesAsync);

        var entries = result.Value.UserEntries.ToDictionary(entry => entry.Id);
        entries["aad.Anna"].AffectedUserRulesCounts.Should().BeEquivalentTo(new TotalSeverityCountsDto { TotalCriticals = 2, TotalWarnings = 1, TotalOks = 1 });
        entries["aad.ben"].AffectedUserRulesCounts.Should().BeEquivalentTo(new TotalSeverityCountsDto { TotalCriticals = 0, TotalWarnings = 1, TotalOks = 2 });
    }

    [Test]
    public async Task GetUserEntries_ReportsZeroCountsForAUserWithoutAuditResults()
    {
        var anna = User("aad.anna");
        GivenAudit(Audit());
        GivenUsers(anna);
        GivenAccessLevel(anna, "active");
        var sut = CreateSut();

        var result = await Run(sut.GetUserEntriesAsync);

        result.Value.UserEntries.Single().AffectedUserRulesCounts
            .Should().BeEquivalentTo(new TotalSeverityCountsDto { TotalCriticals = 0, TotalWarnings = 0, TotalOks = 0 });
    }

    [Test]
    public async Task GetUserEntries_CarriesTheAuditItWasComputedFrom()
    {
        var audit = Audit();
        GivenAudit(audit);
        GivenUsers();
        var sut = CreateSut();

        var result = await Run(sut.GetUserEntriesAsync);

        result.Value.Audit.Should().BeSameAs(audit);
    }

    // ---- GetUserByIdAsync ----

    [Test]
    public async Task GetUserById_PropagatesAFailedAudit()
    {
        _auditRepository
            .Setup(repository => repository.GetAuditByIdAsync(AuditKey))
            .ReturnsAsync(Result.Fail<Audit>(new AuditExpiredError(AuditKey)));
        var sut = CreateSut();

        var result = await Run(() => sut.GetUserByIdAsync("aad.anna"));

        result.Errors.Single().Should().BeOfType<AuditExpiredError>();
    }

    [Test]
    public async Task GetUserById_FailsWithUserNotFoundForAnUnknownDescriptor()
    {
        GivenAudit(Audit());
        _userRepository.Setup(repository => repository.GetUserByDescriptorAsync("aad.ghost")).ReturnsAsync((UserDto?)null);
        var sut = CreateSut();

        var result = await Run(() => sut.GetUserByIdAsync("aad.ghost"));

        var error = result.Errors.Single().Should().BeOfType<UserNotFoundError>().Subject;
        error.Metadata["userId"].Should().Be("aad.ghost");
    }

    [Test]
    public async Task GetUserById_FailsWithAccessLevelNotFoundWhenTheUserHasNoEntitlement()
    {
        var anna = User("aad.anna");
        GivenAudit(Audit());
        GivenUsers(anna);
        GivenAccessLevel(anna, null);
        var sut = CreateSut();

        var result = await Run(() => sut.GetUserByIdAsync("aad.anna"));

        var error = result.Errors.Single().Should().BeOfType<AccessLevelNotFoundError>().Subject;
        error.Metadata["userId"].Should().Be("aad.anna");
    }

    [Test]
    public async Task GetUserById_ReturnsEmptyPatListAndZeroSummaryForAUserWithoutTokens()
    {
        var anna = User("aad.anna", "Anna", "guest");
        GivenAudit(Audit());
        GivenUsers(anna);
        GivenAccessLevel(anna, "active");
        var sut = CreateSut();

        var result = await Run(() => sut.GetUserByIdAsync("aad.anna"));

        result.IsSuccess.Should().BeTrue();
        result.Value.User.Id.Should().Be("aad.anna");
        result.Value.User.UserName.Should().Be("Anna");
        result.Value.User.MetaType.Should().Be("guest");
        result.Value.User.IsActive.Should().BeTrue();
        result.Value.Pats.Should().BeEmpty();
        result.Value.PatsSummary.Should().BeEquivalentTo(new TotalSeverityCountsDto { TotalCriticals = 0, TotalWarnings = 0, TotalOks = 0 });
        result.Value.Rules.Should().BeEmpty();
    }

    [Test]
    public async Task GetUserById_ListsOnlyTheTokensOwnedByTheRequestedUser()
    {
        var anna = User("aad.anna", "Anna");
        var ben = User("aad.ben", "Ben");
        var annasPat = Token("pat-anna", "uid-anna", "Annas PAT");
        var bensPat = Token("pat-ben", "uid-ben", "Bens PAT");
        var annasKey = Token("ssh-anna", "uid-anna", "Annas key");
        GivenAudit(Audit(
            pats: [ResultFor(annasPat, DomainSeverities.Critical), ResultFor(bensPat, DomainSeverities.Critical, DomainSeverities.Critical)],
            sshKeys: [ResultFor(annasKey, DomainSeverities.Warning)]));
        GivenUsers(anna, ben);
        GivenAccessLevel(anna, "active");
        GivenTokenOwner("uid-anna", User("AAD.ANNA"));
        GivenTokenOwner("uid-ben", ben);
        var sut = CreateSut();

        var result = await Run(() => sut.GetUserByIdAsync("aad.anna"));

        result.Value.Pats.Select(pat => pat.Id).Should().BeEquivalentTo(["pat-anna", "ssh-anna"]);
        result.Value.PatsSummary.Should().BeEquivalentTo(new TotalSeverityCountsDto { TotalCriticals = 1, TotalWarnings = 1, TotalOks = 0 });
    }

    [Test]
    public async Task GetUserById_MapsTokenDetailsAndCountsSeveritiesPerToken()
    {
        var anna = User("aad.anna", "Anna");
        var first = Token("pat-1", "uid-anna", "First", "2026-01-02T03:04:05", "2026-07-08T09:10:11", "vso.code vso.build_execute");
        var second = Token("pat-2", "uid-anna", "Second", "2026-02-03T04:05:06", "2026-08-09T10:11:12", "vso.work");
        GivenAudit(Audit(pats:
        [
            ResultFor(first, DomainSeverities.Critical, DomainSeverities.Warning, DomainSeverities.Warning),
            ResultFor(second, DomainSeverities.Ok),
        ]));
        GivenUsers(anna);
        GivenAccessLevel(anna, "active");
        GivenTokenOwner("uid-anna", anna);
        var sut = CreateSut();

        var result = await Run(() => sut.GetUserByIdAsync("aad.anna"));

        var pats = result.Value.Pats.ToDictionary(pat => pat.Id);
        var firstPat = pats["pat-1"];
        firstPat.Name.Should().Be("First");
        firstPat.User.UserId.Should().Be("aad.anna");
        firstPat.User.Name.Should().Be("Anna");
        firstPat.CreatedAt.Should().Be("2026-01-02 03:04:05Z");
        firstPat.ExpiresIn.Should().Be("2026-07-08 09:10:11Z");
        firstPat.Scopes.Should().Equal("vso.code", "vso.build_execute");
        firstPat.AffectedPatRulesCounts.Should().BeEquivalentTo(new TotalSeverityCountsDto { TotalCriticals = 1, TotalWarnings = 2, TotalOks = 0 });
        pats["pat-2"].Scopes.Should().Equal("vso.work");
        pats["pat-2"].AffectedPatRulesCounts.Should().BeEquivalentTo(new TotalSeverityCountsDto { TotalCriticals = 0, TotalWarnings = 0, TotalOks = 1 });
        result.Value.PatsSummary.Should().BeEquivalentTo(new TotalSeverityCountsDto { TotalCriticals = 1, TotalWarnings = 2, TotalOks = 1 });
    }

    [Test]
    public async Task GetUserById_AsksForRulesOfTheUsersOwnAuditResultsOnly()
    {
        var anna = User("aad.anna");
        var annasRule = Rule(Guid.NewGuid(), DomainSeverities.Critical, DomainCategories.Users);
        var bensRule = Rule(Guid.NewGuid(), DomainSeverities.Warning, DomainCategories.Users);
        GivenAudit(Audit(users:
        [
            ResultFor(User("AAD.ANNA"), annasRule),
            ResultFor(User("aad.ben"), bensRule),
        ]));
        GivenUsers(anna);
        GivenAccessLevel(anna, "active");
        var expected = new RuleDto
        {
            Id = Guid.NewGuid(),
            Scope = AdoExport.Api.Controllers.Models.Categories.Users,
            Severity = AdoExport.Api.Controllers.Models.Severities.Critical,
            Name = "n",
            Description = "d",
            Recommendation = "r",
            TotalCounts = 1,
        };
        ImmutableArray<RuleResult> asked = default;
        var sut = CreateSut();
        _rulesRepository
            .Setup(rules => rules.GetRuleDtosForAsync(It.IsAny<ImmutableArray<RuleResult>>()))
            .Callback((ImmutableArray<RuleResult> given) => asked = given)
            .ReturnsAsync([expected]);

        var result = await Run(() => sut.GetUserByIdAsync("aad.anna"));

        asked.Should().Equal(annasRule);
        result.Value.Rules.Should().Equal(expected);
    }

    [Test]
    public async Task GetUserById_DoesNotCrashOnATokenWhoseOwnerIsUnknown()
    {
        // A token in the audit whose owner cannot be resolved belongs to nobody we can
        // show; it must not take the whole user page down with a NullReferenceException.
        var anna = User("aad.anna", "Anna");
        GivenAudit(Audit(pats:
        [
            ResultFor(Token("pat-orphan", "uid-unknown"), DomainSeverities.Critical),
            ResultFor(Token("pat-anna", "uid-anna"), DomainSeverities.Warning),
        ]));
        GivenUsers(anna);
        GivenAccessLevel(anna, "active");
        GivenTokenOwner("uid-unknown", null);
        GivenTokenOwner("uid-anna", anna);
        var sut = CreateSut();

        var result = await Run(() => sut.GetUserByIdAsync("aad.anna"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Pats.Should().ContainSingle().Which.Id.Should().Be("pat-anna");
        result.Value.PatsSummary.TotalCriticals.Should().Be(0);
    }

    // ---- GetAllUsersByIdAsync ----

    [Test]
    public async Task GetAllUsersById_ReturnsEmptyDictionaryForAnOrganizationWithoutUsers()
    {
        GivenAudit(Audit());
        GivenUsers();
        var sut = CreateSut();

        var result = await Run(sut.GetAllUsersByIdAsync);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllUsersById_KeysEveryResolvableUserByItsDescriptor()
    {
        var anna = User("aad.anna", "Anna");
        var ben = User("aad.ben", "Ben");
        GivenAudit(Audit());
        GivenUsers(anna, ben);
        GivenAccessLevel(anna, "active");
        GivenAccessLevel(ben, "disabled");
        var sut = CreateSut();

        var result = await Run(sut.GetAllUsersByIdAsync);

        result.Value.Keys.Should().BeEquivalentTo(["aad.anna", "aad.ben"]);
        result.Value["aad.anna"].User.IsActive.Should().BeTrue();
        result.Value["aad.ben"].User.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task GetAllUsersById_LeavesOutUsersThatCannotBeResolvedInsteadOfFailing()
    {
        var licensed = User("aad.licensed");
        var agent = User("build.agent", metaType: "service");
        GivenAudit(Audit());
        GivenUsers(licensed, agent);
        GivenAccessLevel(licensed, "active");
        GivenAccessLevel(agent, null);
        var sut = CreateSut();

        var result = await Run(sut.GetAllUsersByIdAsync);

        result.IsSuccess.Should().BeTrue();
        result.Value.Keys.Should().Equal("aad.licensed");
    }

    [Test]
    public async Task GetUserEntries_LogsAWarningForEachUserItSkipsBecauseOfAMissingAccessLevel()
    {
        var unlicensed = User("aad.unlicensed");
        GivenAudit(Audit());
        GivenUsers(unlicensed);
        GivenAccessLevel(unlicensed, null);
        var logger = new CapturingLogger<UsersFacade>();
        var sut = CreateSut(logger);

        await Run(sut.GetUserEntriesAsync);

        logger.Entries.Should().ContainSingle(entry => entry.Level == LogLevel.Warning && entry.Message.Contains("aad.unlicensed"));
    }

    [Test]
    public async Task GetUserById_LogsAWarningWhenTheUserHasNoAccessLevel()
    {
        var unlicensed = User("aad.unlicensed");
        GivenAudit(Audit());
        GivenUsers(unlicensed);
        GivenAccessLevel(unlicensed, null);
        var logger = new CapturingLogger<UsersFacade>();
        var sut = CreateSut(logger);

        await Run(() => sut.GetUserByIdAsync("aad.unlicensed"));

        logger.Entries.Should().ContainSingle(entry => entry.Level == LogLevel.Warning && entry.Message.Contains("aad.unlicensed"));
    }

    [Test]
    public async Task GetAllUsersById_KeepsASingleEntryWhenTheOrganizationListsADescriptorTwice()
    {
        var anna = User("aad.anna", "Anna");
        GivenAudit(Audit());
        GivenUsers(anna, anna);
        GivenAccessLevel(anna, "active");
        var logger = new CapturingLogger<UsersFacade>();
        var sut = CreateSut(logger);

        var result = await Run(sut.GetAllUsersByIdAsync);

        result.Value.Keys.Should().Equal("aad.anna");
        logger.Entries.Should().Contain(entry => entry.Level == LogLevel.Warning && entry.Message.Contains("Failed to add user by id result"));
    }
    [Test]
    public void RejectsMissingCollaborators()
    {
        var logger = NullLogger<UsersFacade>.Instance;

        FluentActions.Invoking(() => new UsersFacade(null!, _auditRepository.Object, _userRepository.Object, _rulesRepository.Object, _bearer))
            .Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new UsersFacade(logger, null!, _userRepository.Object, _rulesRepository.Object, _bearer))
            .Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new UsersFacade(logger, _auditRepository.Object, null!, _rulesRepository.Object, _bearer))
            .Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new UsersFacade(logger, _auditRepository.Object, _userRepository.Object, null!, _bearer))
            .Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new UsersFacade(logger, _auditRepository.Object, _userRepository.Object, _rulesRepository.Object, null!))
            .Should().Throw<ArgumentNullException>();
    }
}
