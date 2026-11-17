using System.Collections.Immutable;

using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Errors;
using AdoExport.Api.Repositories;
using AdoExport.Api.Services;
using AdoExport.Auditing;
using AdoExport.Auditing.Rules;
using AdoExport.Client.AdoApi.Models;

using FluentResults;

using Microsoft.Extensions.Logging.Abstractions;

using static AdoExport.Api.Tests.Repositories.RepositoryTestData;

using DomainSeverities = AdoExport.Auditing.Severities;

namespace AdoExport.Api.Tests.Repositories;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class PatsFacadeTests
{
    private readonly Mock<IAuditRepository> _auditRepository = new();

    private readonly Mock<IUserRepository> _userRepository = new();

    private readonly Mock<IRulesRepository> _rulesRepository = new();

    private readonly ScopedAuditKeyBearer _bearer = new();

    private PatsFacade CreateSut()
    {
        _rulesRepository
            .Setup(rules => rules.GetRuleDtosForAsync(It.IsAny<ImmutableArray<AuditResult>>(), It.IsAny<ImmutableArray<AuditResult>>()))
            .ReturnsAsync([]);

        return new PatsFacade(
            NullLogger<PatsFacade>.Instance,
            _auditRepository.Object,
            _userRepository.Object,
            _rulesRepository.Object,
            _bearer);
    }

    private Task<T> Run<T>(Func<Task<T>> action) => InScopeAsync(_bearer, action);

    private void GivenAudit(Audit audit)
        => _auditRepository.Setup(repository => repository.GetAuditByIdAsync(AuditKey)).ReturnsAsync(Result.Ok(audit));

    private void GivenTokenOwner(string userId, UserDto owner)
        => _userRepository.Setup(repository => repository.GetUserByUserIdAsync(userId)).ReturnsAsync(owner);

    // ---- GetAllPatsAsync ----

    [Test]
    public async Task GetAllPats_PropagatesAFailedAudit()
    {
        _auditRepository
            .Setup(repository => repository.GetAuditByIdAsync(AuditKey))
            .ReturnsAsync(Result.Fail<Audit>(new AuditExpiredError(AuditKey)));
        var sut = CreateSut();

        var result = await Run(sut.GetAllPatsAsync);

        result.Errors.Single().Should().BeOfType<AuditExpiredError>();
    }

    [Test]
    public async Task GetAllPats_ReturnsNoPatsForAnAuditWithoutTokens()
    {
        var audit = Audit();
        GivenAudit(audit);
        var sut = CreateSut();

        var result = await Run(sut.GetAllPatsAsync);

        result.IsSuccess.Should().BeTrue();
        result.Value.Pats.Should().BeEmpty();
        result.Value.Audit.Should().BeSameAs(audit);
    }

    [Test]
    public async Task GetAllPats_ListsPatsAndSshKeysTogether()
    {
        var owner = User("aad.anna", "Anna");
        GivenAudit(Audit(
            pats: [ResultFor(Token("pat-1", "uid"), DomainSeverities.Ok)],
            sshKeys: [ResultFor(Token("ssh-1", "uid"), DomainSeverities.Ok)]));
        GivenTokenOwner("uid", owner);
        var sut = CreateSut();

        var result = await Run(sut.GetAllPatsAsync);

        result.Value.Pats.Select(pat => pat.Id).Should().BeEquivalentTo(["pat-1", "ssh-1"]);
    }

    [Test]
    public async Task GetAllPats_MapsTokenOwnerDatesAndScopes()
    {
        var owner = User("aad.anna", "Anna");
        GivenAudit(Audit(pats:
        [
            ResultFor(
                Token("pat-1", "uid", "Deploy token", "2026-03-04T05:06:07", "2026-09-10T11:12:13", "vso.code vso.packaging vso.release"),
                DomainSeverities.Ok),
        ]));
        GivenTokenOwner("uid", owner);
        var sut = CreateSut();

        var result = await Run(sut.GetAllPatsAsync);

        var pat = result.Value.Pats.Single();
        pat.Name.Should().Be("Deploy token");
        pat.User.UserId.Should().Be("aad.anna");
        pat.User.Name.Should().Be("Anna");
        pat.CreatedAt.Should().Be("2026-03-04 05:06:07Z");
        pat.ExpiresIn.Should().Be("2026-09-10 11:12:13Z");
        pat.Scopes.Should().Equal("vso.code", "vso.packaging", "vso.release");
    }

    [Test]
    public async Task GetAllPats_CountsSeveritiesPerTokenWithoutMixingTokensUp()
    {
        var owner = User("aad.anna");
        GivenAudit(Audit(
            pats:
            [
                ResultFor(Token("pat-1", "uid"), DomainSeverities.Critical, DomainSeverities.Critical, DomainSeverities.Warning),
                ResultFor(Token("pat-2", "uid"), DomainSeverities.Ok, DomainSeverities.Ok),
            ],
            sshKeys: [ResultFor(Token("ssh-1", "uid"), DomainSeverities.Warning)]));
        GivenTokenOwner("uid", owner);
        var sut = CreateSut();

        var result = await Run(sut.GetAllPatsAsync);

        var pats = result.Value.Pats.ToDictionary(pat => pat.Id);
        pats["pat-1"].AffectedPatRulesCounts.Should().BeEquivalentTo(new TotalSeverityCountsDto { TotalCriticals = 2, TotalWarnings = 1, TotalOks = 0 });
        pats["pat-2"].AffectedPatRulesCounts.Should().BeEquivalentTo(new TotalSeverityCountsDto { TotalCriticals = 0, TotalWarnings = 0, TotalOks = 2 });
        pats["ssh-1"].AffectedPatRulesCounts.Should().BeEquivalentTo(new TotalSeverityCountsDto { TotalCriticals = 0, TotalWarnings = 1, TotalOks = 0 });
    }

    [Test]
    public async Task GetAllPats_ResolvesEachTokensOwnerByTheTokensUserId()
    {
        GivenAudit(Audit(pats:
        [
            ResultFor(Token("pat-anna", "uid-anna"), DomainSeverities.Ok),
            ResultFor(Token("pat-ben", "uid-ben"), DomainSeverities.Ok),
        ]));
        GivenTokenOwner("uid-anna", User("aad.anna", "Anna"));
        GivenTokenOwner("uid-ben", User("aad.ben", "Ben"));
        var sut = CreateSut();

        var result = await Run(sut.GetAllPatsAsync);

        var pats = result.Value.Pats.ToDictionary(pat => pat.Id);
        pats["pat-anna"].User.UserId.Should().Be("aad.anna");
        pats["pat-ben"].User.UserId.Should().Be("aad.ben");
    }

    // ---- GetPatByIdAsync ----

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task GetPatById_RejectsABlankPatId(string? patId)
    {
        var sut = CreateSut();

        var act = () => Run(() => sut.GetPatByIdAsync(patId!));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task GetPatById_PropagatesAFailedAudit()
    {
        _auditRepository
            .Setup(repository => repository.GetAuditByIdAsync(AuditKey))
            .ReturnsAsync(Result.Fail<Audit>(new AuditExpiredError(AuditKey)));
        var sut = CreateSut();

        var result = await Run(() => sut.GetPatByIdAsync("pat-1"));

        result.Errors.Single().Should().BeOfType<AuditExpiredError>();
    }

    [Test]
    public async Task GetPatById_FailsWithPatNotFoundForAnUnknownId()
    {
        GivenAudit(Audit(pats: [ResultFor(Token("pat-1", "uid"), DomainSeverities.Ok)]));
        var sut = CreateSut();

        var result = await Run(() => sut.GetPatByIdAsync("pat-other"));

        var error = result.Errors.Single().Should().BeOfType<PatNotFoundError>().Subject;
        error.Metadata["patId"].Should().Be("pat-other");
    }

    [Test]
    public async Task GetPatById_FailsWithPatNotFoundWhenThereAreNoTokensAtAll()
    {
        GivenAudit(Audit());
        var sut = CreateSut();

        var result = await Run(() => sut.GetPatByIdAsync("pat-1"));

        result.Errors.Single().Should().BeOfType<PatNotFoundError>();
    }

    [Test]
    public async Task GetPatById_MatchesTheIdExactlyAndCaseSensitively()
    {
        GivenAudit(Audit(pats: [ResultFor(Token("PAT-1", "uid"), DomainSeverities.Ok)]));
        var sut = CreateSut();

        var result = await Run(() => sut.GetPatByIdAsync("pat-1"));

        result.Errors.Single().Should().BeOfType<PatNotFoundError>();
    }

    [Test]
    public async Task GetPatById_FindsAnSshKeyToo()
    {
        GivenAudit(Audit(sshKeys: [ResultFor(Token("ssh-1", "uid", "Deploy key"), DomainSeverities.Warning)]));
        GivenTokenOwner("uid", User("aad.anna", "Anna"));
        var sut = CreateSut();

        var result = await Run(() => sut.GetPatByIdAsync("ssh-1"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Pat.Name.Should().Be("Deploy key");
        result.Value.Pat.AffectedPatRulesCounts.TotalWarnings.Should().Be(1);
    }

    [Test]
    public async Task GetPatById_ReturnsOnlyTheRequestedTokensDetailsAndCounts()
    {
        var owner = User("aad.anna", "Anna");
        var audit = Audit(pats:
        [
            ResultFor(Token("pat-1", "uid", "One", "2026-03-04T05:06:07", "2026-09-10T11:12:13", "vso.code vso.work"), DomainSeverities.Critical, DomainSeverities.Ok),
            ResultFor(Token("pat-2", "uid", "Two"), DomainSeverities.Critical, DomainSeverities.Critical, DomainSeverities.Critical),
        ]);
        GivenAudit(audit);
        GivenTokenOwner("uid", owner);
        var sut = CreateSut();

        var result = await Run(() => sut.GetPatByIdAsync("pat-1"));

        result.Value.Audit.Should().BeSameAs(audit);
        var pat = result.Value.Pat;
        pat.Id.Should().Be("pat-1");
        pat.Name.Should().Be("One");
        pat.User.UserId.Should().Be("aad.anna");
        pat.User.Name.Should().Be("Anna");
        pat.CreatedAt.Should().Be("2026-03-04 05:06:07Z");
        pat.ExpiresIn.Should().Be("2026-09-10 11:12:13Z");
        pat.Scopes.Should().Equal("vso.code", "vso.work");
        pat.AffectedPatRulesCounts.Should().BeEquivalentTo(new TotalSeverityCountsDto { TotalCriticals = 1, TotalWarnings = 0, TotalOks = 1 });
    }

    [Test]
    public async Task GetPatById_PassesTheTokensOwnResultsAndAllResultsToTheRulesRepository()
    {
        var mine = ResultFor(Token("pat-1", "uid"), DomainSeverities.Critical);
        var others = ResultFor(Token("pat-2", "uid"), DomainSeverities.Warning);
        var key = ResultFor(Token("ssh-1", "uid"), DomainSeverities.Ok);
        GivenAudit(Audit(pats: [mine, others], sshKeys: [key]));
        GivenTokenOwner("uid", User("aad.anna"));
        ImmutableArray<AuditResult> askedFor = default;
        ImmutableArray<AuditResult> askedAgainst = default;
        var expected = new RuleDto
        {
            Id = Guid.NewGuid(),
            Scope = AdoExport.Api.Controllers.Models.Categories.Pats,
            Severity = AdoExport.Api.Controllers.Models.Severities.Critical,
            Name = "n",
            Description = "d",
            Recommendation = "r",
            TotalCounts = 1,
        };
        var sut = CreateSut();
        _rulesRepository
            .Setup(rules => rules.GetRuleDtosForAsync(It.IsAny<ImmutableArray<AuditResult>>(), It.IsAny<ImmutableArray<AuditResult>>()))
            .Callback((ImmutableArray<AuditResult> forThese, ImmutableArray<AuditResult> all) =>
            {
                askedFor = forThese;
                askedAgainst = all;
            })
            .ReturnsAsync([expected]);

        var result = await Run(() => sut.GetPatByIdAsync("pat-1"));

        askedFor.Should().Equal(mine);
        askedAgainst.Should().Equal(mine, others, key);
        result.Value.Rules.Should().Equal(expected);
    }

    // ---- GetAllPatsByIdAsync ----

    [Test]
    public async Task GetAllPatsById_PropagatesAFailedAudit()
    {
        _auditRepository
            .Setup(repository => repository.GetAuditByIdAsync(AuditKey))
            .ReturnsAsync(Result.Fail<Audit>(new AuditExpiredError(AuditKey)));
        var sut = CreateSut();

        var result = await Run(sut.GetAllPatsByIdAsync);

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Should().BeOfType<AuditExpiredError>();
    }

    [Test]
    public async Task GetAllPatsById_ReturnsEmptyDictionaryWithoutTokens()
    {
        GivenAudit(Audit());
        var sut = CreateSut();

        var result = await Run(sut.GetAllPatsByIdAsync);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllPatsById_KeysEveryTokenByItsAuthorizationId()
    {
        GivenAudit(Audit(
            pats: [ResultFor(Token("pat-1", "uid", "One"), DomainSeverities.Critical), ResultFor(Token("pat-2", "uid", "Two"), DomainSeverities.Ok)],
            sshKeys: [ResultFor(Token("ssh-1", "uid", "Key"), DomainSeverities.Warning)]));
        GivenTokenOwner("uid", User("aad.anna"));
        var sut = CreateSut();

        var result = await Run(sut.GetAllPatsByIdAsync);

        result.Value.Keys.Should().BeEquivalentTo(["pat-1", "pat-2", "ssh-1"]);
        result.Value["pat-1"].Pat.Name.Should().Be("One");
        result.Value["pat-2"].Pat.Name.Should().Be("Two");
        result.Value["ssh-1"].Pat.Name.Should().Be("Key");
    }

    [Test]
    public async Task GetAllPatsById_KeepsOneEntryWhenAnIdAppearsAmongPatsAndSshKeys()
    {
        GivenAudit(Audit(
            pats: [ResultFor(Token("same", "uid"), DomainSeverities.Ok)],
            sshKeys: [ResultFor(Token("same", "uid"), DomainSeverities.Ok)]));
        GivenTokenOwner("uid", User("aad.anna"));
        var sut = CreateSut();

        var result = await Run(sut.GetAllPatsByIdAsync);

        result.IsSuccess.Should().BeTrue();
        result.Value.Keys.Should().Equal("same");
    }

    [Test]
    public async Task GetAllPatsById_LeavesOutTokensThatCannotBeResolvedInsteadOfFailing()
    {
        // The overview resolves; every later per-token lookup finds the audit gone.
        GivenAudit(Audit(pats: [ResultFor(Token("pat-1", "uid"), DomainSeverities.Ok)]));
        GivenTokenOwner("uid", User("aad.anna"));
        _auditRepository
            .SetupSequence(repository => repository.GetAuditByIdAsync(AuditKey))
            .ReturnsAsync(Result.Ok(Audit(pats: [ResultFor(Token("pat-1", "uid"), DomainSeverities.Ok)])))
            .ReturnsAsync(Result.Fail<Audit>(new AuditExpiredError(AuditKey)));
        var sut = CreateSut();

        var result = await Run(sut.GetAllPatsByIdAsync);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public void RejectsMissingCollaborators()
    {
        var logger = NullLogger<PatsFacade>.Instance;

        FluentActions.Invoking(() => new PatsFacade(null!, _auditRepository.Object, _userRepository.Object, _rulesRepository.Object, _bearer))
            .Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new PatsFacade(logger, null!, _userRepository.Object, _rulesRepository.Object, _bearer))
            .Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new PatsFacade(logger, _auditRepository.Object, null!, _rulesRepository.Object, _bearer))
            .Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new PatsFacade(logger, _auditRepository.Object, _userRepository.Object, null!, _bearer))
            .Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new PatsFacade(logger, _auditRepository.Object, _userRepository.Object, _rulesRepository.Object, null!))
            .Should().Throw<ArgumentNullException>();
    }
}
