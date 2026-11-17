using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Reports.Models;
using AdoExport.Api.Reports.Templates.Components;
using AdoExport.Api.Reports.Templates.Components.Layout;
using AdoExport.Api.Reports.Templates.Components.Layout.TableOfContents;
using AdoExport.Api.Reports.Templates.Components.PATs;
using AdoExport.Api.Reports.Templates.Components.Rules;
using AdoExport.Api.Reports.Templates.Components.Summary;
using AdoExport.Api.Reports.Templates.Components.Users;

namespace AdoExport.Api.Tests.Reports;

/// <summary>
/// Renders single components on their own. The end-to-end report tests cover the common
/// path; these reach the branches that path only touches by accident or not at all.
/// </summary>
[TestFixture]
public class ReportComponentTests
{
    private static readonly Severities[] AllSeverities = [Severities.Critical, Severities.Warning, Severities.Ok];

    private static byte[] Render(QuestPDF.Infrastructure.IComponent component) => ReportRenderer.RenderComponent(component);

    [TestCaseSource(nameof(AllSeverities))]
    public void AffectedCountRendersForEverySeverityWithAndWithoutFindings(Severities severity)
    {
        PdfInspector.LooksLikePdf(Render(new AffectedCountComponent(severity, 7))).Should().BeTrue();
        PdfInspector.LooksLikePdf(Render(new AffectedCountComponent(severity, 0))).Should().BeTrue();
    }

    [TestCaseSource(nameof(AllSeverities))]
    public void SeverityIconRendersWithAndWithoutLabel(Severities severity)
    {
        PdfInspector.LooksLikePdf(Render(new SeverityIconComponent(severity))).Should().BeTrue();
        PdfInspector.LooksLikePdf(Render(new SeverityIconComponent(severity, "Critical"))).Should().BeTrue();
    }

    [Test]
    public void SeverityIconWithALabelIsLargerThanOneWithout()
    {
        var plain = Render(new SeverityIconComponent(Severities.Warning));
        var labelled = Render(new SeverityIconComponent(Severities.Warning, "Warning"));

        labelled.Length.Should().BeGreaterThan(plain.Length);
    }

    [TestCase(Headings.H1)]
    [TestCase(Headings.H2)]
    [TestCase(Headings.H3)]
    public void SectionHeadingRendersEveryLevel(Headings heading)
    {
        PdfInspector.LooksLikePdf(Render(new SectionHeadingComponent("Title", heading))).Should().BeTrue();
    }

    [Test]
    public void SectionHeadingRendersSeverityCounts()
    {
        var counts = AllSeverities.Select(severity => new SeverityCounts { Severity = severity, Count = 3 }).ToArray();

        var withCounts = Render(new SectionHeadingComponent("Title", Headings.H2, counts));
        var without = Render(new SectionHeadingComponent("Title", Headings.H2));

        withCounts.Length.Should().BeGreaterThan(without.Length);
    }

    [Test]
    public void SectionHeadingRejectsAnUnknownLevel()
    {
        var sut = new SectionHeadingComponent("Title", (Headings)42);

        Assert.That(() => Render(sut), Throws.Exception);
    }

    [TestCase("")]
    [TestCase("   ")]
    public void SectionHeadingRejectsABlankTitle(string title)
    {
        Assert.Throws<ArgumentException>(() => new SectionHeadingComponent(title, Headings.H1));
    }

    [Test]
    public void DetailLabelRenders()
    {
        PdfInspector.LooksLikePdf(Render(new DetailLabelComponent("Name", "Alice"))).Should().BeTrue();
    }

    [Test]
    public void PatTableShowsAMessageInsteadOfATableWhenThereAreNoPats()
    {
        var empty = Render(new PatTableComponent([], showUser: false));
        var filled = Render(new PatTableComponent([ReportTestData.Pat("p1", "Token")], showUser: false));

        PdfInspector.LooksLikePdf(empty).Should().BeTrue();
        PdfInspector.LinkTargets(empty).Should().BeEmpty();
        PdfInspector.LinkTargets(filled).Should().Contain("pat-p1");
    }

    [Test]
    public void PatTableLinksToTheOwningUserOnlyWhenTheUserColumnIsShown()
    {
        var pats = new[] { ReportTestData.Pat("p1", "Token", userId: "owner-9") };

        var withUser = PdfInspector.LinkTargets(Render(new PatTableComponent(pats, showUser: true)));
        var withoutUser = PdfInspector.LinkTargets(Render(new PatTableComponent(pats, showUser: false)));

        withUser.Should().Contain("user-owner-9");
        withoutUser.Should().NotContain("user-owner-9");
    }

    [Test]
    public void PatScopesRendersOneEntryPerScope()
    {
        PdfInspector.LooksLikePdf(Render(new PatScopesComponent(["a", "b", "c"]))).Should().BeTrue();
        PdfInspector.LooksLikePdf(Render(new PatScopesComponent([]))).Should().BeTrue();
    }

    [Test]
    public void PatDetailsLinkToTheUserWhoCreatedThePat()
    {
        var pdf = Render(new PatDetailsSectionComponent(ReportTestData.Pat("p1", "Token", userId: "creator-3")));

        PdfInspector.LinkTargets(pdf).Should().Contain("user-creator-3");
    }

    [Test]
    public void PatSectionRendersWithAndWithoutRules()
    {
        var pat = ReportTestData.Pat("p1", "Token");
        var rules = new[] { ReportTestData.Rule("Rule", Severities.Critical, Categories.Pats) };

        var withRules = Render(new PatSectionComponent(ReportTestData.PatById(pat, rules)));
        var withoutRules = Render(new PatSectionComponent(ReportTestData.PatById(pat, [])));

        PdfInspector.LinkTargets(withRules).Should().Contain("user-user-1");
        PdfInspector.LinkTargets(withRules).Should().Contain(t => t.StartsWith("rule-"));
        PdfInspector.LinkTargets(withoutRules).Should().NotContain(t => t.StartsWith("rule-"));
    }

    [Test]
    public void PatsOverviewAndPatsByIdRender()
    {
        var pat = ReportTestData.Pat("p1", "Token");

        var overview = Render(new PatsOverviewSectionComponent("pats-overview", new PatsOverview { Pats = [pat] }));
        var byId = Render(new PatsByIdSectionComponent("pats-by-id", new PatsById
        {
            Pats = new Dictionary<string, AdoExport.Api.Repositories.GetPatById> { ["p1"] = ReportTestData.PatById(pat, []) },
        }));

        PdfInspector.LinkTargets(overview).Should().Contain("pat-p1");
        PdfInspector.LinkTargets(byId).Should().Contain("user-user-1");
    }

    [Test]
    public void SectionComponentsRejectBlankSectionIds()
    {
        Assert.Throws<ArgumentException>(() => new PatsOverviewSectionComponent(" ", new PatsOverview { Pats = [] }));
        Assert.Throws<ArgumentException>(() => new PatsByIdSectionComponent("", new PatsById { Pats = [] }));
        Assert.Throws<ArgumentException>(() => new UsersOverviewSectionComponent(" ", new UsersOverview { Users = [] }));
        Assert.Throws<ArgumentException>(() => new UsersByIdSectionComponent("", new UsersById { Users = [] }));
        Assert.Throws<ArgumentException>(() => new RulesOverviewSectionComponent(" ", new RulesOverview { Rules = [] }));
        Assert.Throws<ArgumentException>(() => new SummaryOverviewSectionComponent("", new UsersOverview { Users = [] }));
    }

    [Test]
    public void ComponentsRejectNullModels()
    {
        Assert.Throws<ArgumentNullException>(() => new AffectedRuleEntryComponent(null!));
        Assert.Throws<ArgumentNullException>(() => new AffectedRuleTableComponent(null!));
        Assert.Throws<ArgumentNullException>(() => new AffectedRulesSectionComponent(null!));
        Assert.Throws<ArgumentNullException>(() => new RuleOverviewEntryComponent(null!));
        Assert.Throws<ArgumentNullException>(() => new RulesOverviewSectionComponent("id", null!));
        Assert.Throws<ArgumentNullException>(() => new PatTableComponent(null!, false));
        Assert.Throws<ArgumentNullException>(() => new PatDetailsSectionComponent(null!));
        Assert.Throws<ArgumentNullException>(() => new PatSectionComponent(null!));
        Assert.Throws<ArgumentNullException>(() => new PatsOverviewSectionComponent("id", null!));
        Assert.Throws<ArgumentNullException>(() => new PatsByIdSectionComponent("id", null!));
        Assert.Throws<ArgumentNullException>(() => new UserDetailsSectionComponent(null!));
        Assert.Throws<ArgumentNullException>(() => new UserSectionComponent(null!));
        Assert.Throws<ArgumentNullException>(() => new UsersOverviewSectionComponent("id", null!));
        Assert.Throws<ArgumentNullException>(() => new UsersByIdSectionComponent("id", null!));
        Assert.Throws<ArgumentNullException>(() => new SummaryOverviewSectionComponent("id", null!));
        Assert.Throws<ArgumentNullException>(() => new CoverComponent(null!));
        Assert.Throws<ArgumentNullException>(() => new CoverPage(null!));
        Assert.Throws<ArgumentNullException>(() => new AuditFooterComponent(null!));
        Assert.Throws<ArgumentNullException>(() => new TableOfContentsComponent(null!));
        Assert.Throws<ArgumentNullException>(() => new TableOfContentsItem(null!));
        Assert.Throws<ArgumentNullException>(() => new TableOfContentsPage(null!));
    }

    [Test]
    public void RuleEntryAndAffectedRuleEntryLinkToTheirRule()
    {
        var rule = ReportTestData.Rule("Rule", Severities.Warning);
        var expected = $"rule-{rule.Id.ToString()[..8]}";

        PdfInspector.LinkTargets(Render(new AffectedRuleEntryComponent(rule))).Should().Contain(t => t.StartsWith(expected));
        PdfInspector.LooksLikePdf(Render(new RuleOverviewEntryComponent(rule))).Should().BeTrue();
    }

    [TestCaseSource(nameof(AllSeverities))]
    public void AffectedRuleTableOrdersAndRendersEverySeverity(Severities severity)
    {
        var rules = AllSeverities.Append(severity).Select(s => ReportTestData.Rule($"Rule {s}", s)).ToArray();

        var pdf = Render(new AffectedRulesSectionComponent(rules));

        PdfInspector.LinkTargets(pdf).Count(t => t.StartsWith("rule-")).Should().Be(rules.Length);
    }

    [Test]
    public void AffectedRuleTableRendersWithoutRules()
    {
        PdfInspector.LooksLikePdf(Render(new AffectedRuleTableComponent([]))).Should().BeTrue();
    }

    [Test]
    public void UserDetailsRenderForActiveAndInactiveUsers()
    {
        var active = Render(new UserDetailsSectionComponent(ReportTestData.UserDetails("u1", "Alice", isActive: true)));
        var inactive = Render(new UserDetailsSectionComponent(ReportTestData.UserDetails("u2", "Bob", isActive: false, metaType: "serviceAccount")));

        PdfInspector.LooksLikePdf(active).Should().BeTrue();
        PdfInspector.LooksLikePdf(inactive).Should().BeTrue();
    }

    [Test]
    public void UserSectionRendersAUserWithoutPatsAndWithoutRules()
    {
        var user = ReportTestData.UserDetails("u1", "Alice");

        var bare = Render(new UserSectionComponent(ReportTestData.UserById(user, [], [])));
        var full = Render(new UserSectionComponent(ReportTestData.UserById(
            user,
            [ReportTestData.Pat("p1", "Token")],
            [ReportTestData.Rule("R", Severities.Critical), ReportTestData.Rule("W", Severities.Warning), ReportTestData.Rule("O", Severities.Ok)])));

        PdfInspector.LinkTargets(bare).Should().BeEmpty();
        PdfInspector.LinkTargets(full).Should().Contain("pat-p1");
        full.Length.Should().BeGreaterThan(bare.Length);
    }

    [Test]
    public void UsersOverviewLinksEveryUserToTheirSection()
    {
        var model = new UsersOverview
        {
            Users =
            [
                ReportTestData.UserEntry("u1", "Alice"),
                ReportTestData.UserEntry("u2", "Bob", isActive: false),
            ],
        };

        var targets = PdfInspector.LinkTargets(Render(new UsersOverviewSectionComponent("users-overview", model)));

        targets.Should().Contain(["user-u1", "user-u2"]);
    }

    [Test]
    public void SummaryOverviewLinksEveryUserToTheirSection()
    {
        var model = new UsersOverview { Users = [ReportTestData.UserEntry("u1", "Alice"), ReportTestData.UserEntry("u2", "Bob")] };

        var targets = PdfInspector.LinkTargets(Render(new SummaryOverviewSectionComponent("summary", model)));

        targets.Should().Contain(["user-u1", "user-u2"]);
    }

    [Test]
    public void TableOfContentsListsRulesUsersAndPatsWithTheirLinks()
    {
        var model = ReportTestData.CreateFullModel();

        var targets = PdfInspector.LinkTargets(Render(new TableOfContentsPage(model)));

        targets.Should().Contain(
        [
            "rules-overview",
            "rule-category-Users",
            "rule-category-Pats",
            "users-overview",
            "user-user-1",
            "pats-overview",
            "pat-pat-1",
            "pat-pat-2",
        ]);
    }

    [Test]
    public void TableOfContentsItemIndentsDeeperLevelsFurther()
    {
        PdfInspector.LinkTargets(Render(new TableOfContentsItem(new TableOfContentsItemModel
        {
            SectionId = "deep",
            Indentation = 3,
            ItemNumber = 12,
            Name = "Deeply nested",
        }))).Should().Contain("deep");
    }

    [Test]
    public void CoverAndLayoutComponentsRender()
    {
        var model = ReportTestData.CreateFullModel();

        PdfInspector.LooksLikePdf(Render(new CoverPage(model))).Should().BeTrue();
        PdfInspector.LooksLikePdf(Render(new CoverComponent(model))).Should().BeTrue();
        PdfInspector.LooksLikePdf(Render(new AuditHeaderComponent(model))).Should().BeTrue();
        PdfInspector.Uris(Render(new AuditFooterComponent(model))).Should().Contain("https://ado-auditor.com");
    }
}
