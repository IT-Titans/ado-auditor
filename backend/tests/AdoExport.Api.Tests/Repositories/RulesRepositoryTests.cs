using System.Collections.Immutable;

using AdoExport.Api.Controllers.Mappings;
using AdoExport.Api.Repositories;
using AdoExport.Api.Services;
using AdoExport.Auditing;

using static AdoExport.Api.Tests.Repositories.RepositoryTestData;

using DomainCategories = AdoExport.Auditing.Categories;
using DomainSeverities = AdoExport.Auditing.Severities;
using DtoCategories = AdoExport.Api.Controllers.Models.Categories;
using DtoSeverities = AdoExport.Api.Controllers.Models.Severities;

namespace AdoExport.Api.Tests.Repositories;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class RulesRepositoryTests
{
    private static readonly Guid RuleA = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

    private static readonly Guid RuleB = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");

    private readonly Mock<INotesOnRulesService> _notes = new();

    private RulesRepository CreateSut()
    {
        _notes
            .Setup(notes => notes.GetByRuleReasonId(It.IsAny<Guid>()))
            .Returns((Guid id) => new RuleHint
            {
                Name = $"name-{id}",
                Description = $"description-{id}",
                Recommendations = $"recommendation-{id}",
            });

        return new RulesRepository(_notes.Object, new AuditMapper());
    }

    [Test]
    public async Task Grouped_CollapsesResultsOfTheSameRuleIntoOneDtoWithTheirCount()
    {
        var sut = CreateSut();

        var rules = await sut.GetRuleDtosForAsync(
        [
            Rule(RuleA, DomainSeverities.Critical),
            Rule(RuleB, DomainSeverities.Warning, DomainCategories.Users),
            Rule(RuleA, DomainSeverities.Critical),
            Rule(RuleA, DomainSeverities.Critical),
        ]);

        rules.Should().HaveCount(2);
        var ruleA = rules.Single(rule => rule.Id == RuleA);
        ruleA.TotalCounts.Should().Be(3);
        ruleA.Severity.Should().Be(DtoSeverities.Critical);
        ruleA.Scope.Should().Be(DtoCategories.Pats);
        var ruleB = rules.Single(rule => rule.Id == RuleB);
        ruleB.TotalCounts.Should().Be(1);
        ruleB.Severity.Should().Be(DtoSeverities.Warning);
        ruleB.Scope.Should().Be(DtoCategories.Users);
    }

    [Test]
    public async Task Grouped_CopiesNameDescriptionAndRecommendationFromTheHintOfThatRule()
    {
        var sut = CreateSut();

        var rules = await sut.GetRuleDtosForAsync([Rule(RuleA, DomainSeverities.Ok)]);

        var rule = rules.Should().ContainSingle().Subject;
        rule.Name.Should().Be($"name-{RuleA}");
        rule.Description.Should().Be($"description-{RuleA}");
        rule.Recommendation.Should().Be($"recommendation-{RuleA}");
    }

    [Test]
    public async Task Grouped_ReturnsNothingForNoResults()
    {
        var sut = CreateSut();

        var rules = await sut.GetRuleDtosForAsync(ImmutableArray<AdoExport.Auditing.Rules.RuleResult>.Empty);

        rules.Should().BeEmpty();
        _notes.Verify(notes => notes.GetByRuleReasonId(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task WithTotals_ReportsTheTotalAcrossAllResultsNotJustTheGivenOnes()
    {
        var sut = CreateSut();
        var mine = Rule(RuleA, DomainSeverities.Critical);

        var rules = await sut.GetRuleDtosForAsync(
            [mine],
            [mine, Rule(RuleA, DomainSeverities.Critical), Rule(RuleA, DomainSeverities.Critical), Rule(RuleB, DomainSeverities.Warning)]);

        var rule = rules.Should().ContainSingle().Subject;
        rule.Id.Should().Be(RuleA);
        rule.TotalCounts.Should().Be(3);
    }

    [Test]
    public async Task WithTotals_KeepsOneDtoPerGivenResultInOrder()
    {
        var sut = CreateSut();
        var first = Rule(RuleB, DomainSeverities.Warning);
        var second = Rule(RuleA, DomainSeverities.Critical);

        var rules = await sut.GetRuleDtosForAsync([first, second], [first, second]);

        rules.Select(rule => rule.Id).Should().Equal(RuleB, RuleA);
        rules.Should().OnlyContain(rule => rule.TotalCounts == 1);
    }

    [Test]
    public async Task WithTotals_ReturnsNothingForNoResults()
    {
        var sut = CreateSut();

        var rules = await sut.GetRuleDtosForAsync(
            ImmutableArray<AdoExport.Auditing.Rules.RuleResult>.Empty,
            [Rule(RuleA, DomainSeverities.Ok)]);

        rules.Should().BeEmpty();
    }

    [Test]
    public async Task AuditResultOverload_FlattensRuleResultsOfAllGivenAuditResults()
    {
        var sut = CreateSut();
        var token1 = Token("p1", "u1");
        var token2 = Token("p2", "u2");
        var token3 = Token("p3", "u3");
        var ruleOnP1 = Rule(RuleA, DomainSeverities.Critical);
        var ruleOnP2 = Rule(RuleB, DomainSeverities.Warning);
        var ruleOnP3 = Rule(RuleA, DomainSeverities.Critical);
        var mine1 = ResultFor(token1, ruleOnP1);
        var mine2 = ResultFor(token2, ruleOnP2);
        var other = ResultFor(token3, ruleOnP3);

        var rules = await sut.GetRuleDtosForAsync(
            [(AuditResult)mine1, mine2],
            [(AuditResult)mine1, mine2, other]);

        rules.Should().HaveCount(2);
        rules.Single(rule => rule.Id == RuleA).TotalCounts.Should().Be(2);
        rules.Single(rule => rule.Id == RuleB).TotalCounts.Should().Be(1);
    }

    [Test]
    public async Task AuditResultOverload_ReturnsNothingForAuditResultsWithoutRuleResults()
    {
        var sut = CreateSut();
        var empty = ResultFor(Token("p1", "u1"), Array.Empty<AdoExport.Auditing.Rules.RuleResult>());

        var rules = await sut.GetRuleDtosForAsync([(AuditResult)empty], [(AuditResult)empty]);

        rules.Should().BeEmpty();
    }

    [Test]
    public void RejectsMissingCollaborators()
    {
        FluentActions.Invoking(() => new RulesRepository(null!, new AuditMapper())).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new RulesRepository(_notes.Object, null!)).Should().Throw<ArgumentNullException>();
    }
}
