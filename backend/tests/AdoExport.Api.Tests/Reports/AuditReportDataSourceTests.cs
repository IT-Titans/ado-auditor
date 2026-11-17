using System.Collections.Immutable;

using AdoExport.Api.Reports.DataSources;
using AdoExport.Api.Repositories;
using AdoExport.Api.Services;
using AdoExport.Auditing;
using AdoExport.Auditing.Rules;
using AdoExport.Client.AdoApi.Models;

using FluentResults;

namespace AdoExport.Api.Tests.Reports;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class AuditReportDataSourceTests
{
    private const string AuditKey = "audit-key";

    private Mock<IAuditRepository> _auditRepository = null!;

    private Mock<IRulesRepository> _rulesRepository = null!;

    private Mock<IUsersFacade> _usersFacade = null!;

    private Mock<IPatsFacade> _patsFacade = null!;

    private ScopedAuditKeyBearer _keyBearer = null!;

    private AuditReportDataSource _sut = null!;

    private Audit _audit = null!;

    [SetUp]
    public void SetUp()
    {
        _auditRepository = new Mock<IAuditRepository>();
        _rulesRepository = new Mock<IRulesRepository>();
        _usersFacade = new Mock<IUsersFacade>();
        _patsFacade = new Mock<IPatsFacade>();
        _keyBearer = new ScopedAuditKeyBearer();
        _sut = new AuditReportDataSource(
            _auditRepository.Object,
            _rulesRepository.Object,
            _usersFacade.Object,
            _patsFacade.Object,
            _keyBearer);

        _audit = ReportTestData.CreateAudit("Contoso");
        _auditRepository.Setup(r => r.GetAuditByIdAsync(AuditKey)).ReturnsAsync(Result.Ok(_audit));
        _usersFacade.Setup(f => f.GetUserEntriesAsync()).ReturnsAsync(Result.Ok(new GetUserEntries
        {
            Audit = _audit,
            UserEntries = [ReportTestData.UserEntry("u1", "Alice")],
        }));
        _usersFacade.Setup(f => f.GetAllUsersByIdAsync()).ReturnsAsync(Result.Ok(new Dictionary<string, GetUserById>
        {
            ["u1"] = ReportTestData.UserById(ReportTestData.UserDetails("u1", "Alice"), [], []),
        }));
        _patsFacade.Setup(f => f.GetAllPatsAsync()).ReturnsAsync(Result.Ok(new GetAllPats
        {
            Audit = _audit,
            Pats = [ReportTestData.Pat("p1", "Token")],
        }));
        _patsFacade.Setup(f => f.GetAllPatsByIdAsync()).ReturnsAsync(Result.Ok(new Dictionary<string, GetPatById>
        {
            ["p1"] = ReportTestData.PatById(ReportTestData.Pat("p1", "Token"), []),
        }));
        _rulesRepository
            .Setup(r => r.GetRuleDtosForAsync(It.IsAny<ImmutableArray<RuleResult>>()))
            .ReturnsAsync([]);
    }

    [Test]
    public void GuardsAgainstNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new AuditReportDataSource(null!, _rulesRepository.Object, _usersFacade.Object, _patsFacade.Object, _keyBearer));
        Assert.Throws<ArgumentNullException>(() => new AuditReportDataSource(_auditRepository.Object, null!, _usersFacade.Object, _patsFacade.Object, _keyBearer));
        Assert.Throws<ArgumentNullException>(() => new AuditReportDataSource(_auditRepository.Object, _rulesRepository.Object, null!, _patsFacade.Object, _keyBearer));
        Assert.Throws<ArgumentNullException>(() => new AuditReportDataSource(_auditRepository.Object, _rulesRepository.Object, _usersFacade.Object, null!, _keyBearer));
        Assert.Throws<ArgumentNullException>(() => new AuditReportDataSource(_auditRepository.Object, _rulesRepository.Object, _usersFacade.Object, _patsFacade.Object, null!));
    }

    [Test]
    public async Task AssemblesTheModelFromAuditUsersAndPats()
    {
        var result = await Load();

        result.IsSuccess.Should().BeTrue();
        var model = result.Value;
        model.Organization.Name.Should().Be("Contoso");
        model.Audit.CreatedAt.Should().Be(ReportTestData.AuditCreatedAt);
        model.UsersOverview.Users.Should().ContainSingle(user => user.UserName == "Alice");
        model.UsersById.Users.Should().ContainKey("u1");
        model.PatsOverview.Pats.Should().ContainSingle(pat => pat.Name == "Token");
        model.PatsById.Pats.Should().ContainKey("p1");
    }

    [Test]
    public async Task AsksForTheAuditIdentifiedByTheScopedKey()
    {
        await Load();

        _auditRepository.Verify(r => r.GetAuditByIdAsync(AuditKey), Times.Once);
    }

    [Test]
    public async Task ResolvesRulesForPatSshKeyAndUserResultsTogether()
    {
        var patRule = RuleResultFor(Categories.Pats);
        var sshRule = RuleResultFor(Categories.Pats);
        var userRule = RuleResultFor(Categories.Users);
        var audit = new Audit(new AuditMetadata
        {
            AuditId = Guid.NewGuid(),
            OrganizationName = "Contoso",
            ExpiresAfter = TimeSpan.FromHours(1),
        })
        {
            PatAuditResults = [new AuditResult<SessionTokenDto> { Auditee = null!, RuleResults = [patRule] }],
            SshKeysAuditResults = [new AuditResult<SessionTokenDto> { Auditee = null!, RuleResults = [sshRule] }],
            UserAuditResults = [new AuditResult<UserDto> { Auditee = null!, RuleResults = [userRule] }],
        };
        _auditRepository.Setup(r => r.GetAuditByIdAsync(AuditKey)).ReturnsAsync(Result.Ok(audit));

        ImmutableArray<RuleResult> passed = default;
        var resolved = ReportTestData.Rule("Resolved", AdoExport.Api.Controllers.Models.Severities.Critical);
        _rulesRepository
            .Setup(r => r.GetRuleDtosForAsync(It.IsAny<ImmutableArray<RuleResult>>()))
            .Callback<ImmutableArray<RuleResult>>(results => passed = results)
            .ReturnsAsync([resolved]);

        var result = await Load();

        passed.Should().BeEquivalentTo([patRule, sshRule, userRule]);
        result.Value.RulesOverview.Rules.Should().ContainSingle().Which.Should().BeSameAs(resolved);
    }

    [Test]
    public async Task ReturnsAnEmptyRuleListWhenNothingWasFound()
    {
        var result = await Load();

        result.Value.RulesOverview.Rules.Should().BeEmpty();
    }

    [Test]
    public async Task StopsWhenTheAuditCannotBeLoaded()
    {
        _auditRepository.Setup(r => r.GetAuditByIdAsync(AuditKey)).ReturnsAsync(Result.Fail<Audit>("no audit"));

        var result = await Load();

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(error => error.Message == "no audit");
        _usersFacade.Verify(f => f.GetUserEntriesAsync(), Times.Never);
    }

    [TestCase("user entries")]
    [TestCase("users by id")]
    [TestCase("pats")]
    [TestCase("pats by id")]
    public async Task ReturnsTheFailureOfWhicheverFacadeCallFailed(string failingStep)
    {
        switch (failingStep)
        {
            case "user entries":
                _usersFacade.Setup(f => f.GetUserEntriesAsync()).ReturnsAsync(Result.Fail<GetUserEntries>(failingStep));
                break;
            case "users by id":
                _usersFacade.Setup(f => f.GetAllUsersByIdAsync()).ReturnsAsync(Result.Fail<Dictionary<string, GetUserById>>(failingStep));
                break;
            case "pats":
                _patsFacade.Setup(f => f.GetAllPatsAsync()).ReturnsAsync(Result.Fail<GetAllPats>(failingStep));
                break;
            case "pats by id":
                _patsFacade.Setup(f => f.GetAllPatsByIdAsync()).ReturnsAsync(Result.Fail<Dictionary<string, GetPatById>>(failingStep));
                break;
        }

        var result = await Load();

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(error => error.Message == failingStep);
        _rulesRepository.Verify(r => r.GetRuleDtosForAsync(It.IsAny<ImmutableArray<RuleResult>>()), Times.Never);
    }

    private Task<Result<Api.Reports.Models.AuditPdfModel>> Load()
        => _keyBearer.ScopedAsync(AuditKey, () => _sut.GetAuditDataAsync(CancellationToken.None));

    private static RuleResult RuleResultFor(Categories category)
        => new() { Reason = new Mock<AuditRuleReason>(category).Object };
}
