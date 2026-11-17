using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Reports.Models;
using AdoExport.Api.Reports.Templates;
using AdoExport.Common.Services;

namespace AdoExport.Api.Tests.Reports;

[TestFixture]
public class AuditPdfReportDocumentTests
{
    [Test]
    public void GuardsAgainstNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() => new AuditPdfReportDocument(null!, ReportTestData.CreateEmptyModel()));
        Assert.Throws<ArgumentNullException>(() => new AuditPdfReportDocument(ReportRenderer.FixedClock(), null!));
    }

    [Test]
    public void MetadataNamesTheOrganizationAndUsesTheInjectedClock()
    {
        var sut = ReportRenderer.CreateDocument(ReportTestData.CreateFullModel("Fabrikam"));

        var metadata = sut.GetMetadata();

        metadata.Title.Should().StartWith("Security Audit Fabrikam");
        metadata.Author.Should().Be("ADO Auditor");
        metadata.Creator.Should().Be("ADO Auditor");
        metadata.CreationDate.Should().Be(ReportRenderer.FixedNow);
        sut.GetSettings().Should().NotBeNull();
    }

    [Test]
    public void FullReportIsAValidPdfWithOnePageBreakPerSection()
    {
        var pdf = ReportRenderer.RenderReport(ReportTestData.CreateFullModel());

        PdfInspector.LooksLikePdf(pdf).Should().BeTrue();

        // Cover, table of contents, rules, users, user details, PATs and PAT details are
        // separated by explicit page breaks, so seven pages is the floor.
        PdfInspector.PageCount(pdf).Should().BeGreaterThanOrEqualTo(7);
        PdfInspector.Title(pdf).Should().StartWith("Security Audit Contoso");
    }

    [Test]
    public void FullReportLinksToEverySectionThatHasData()
    {
        var model = ReportTestData.CreateFullModel();

        var targets = PdfInspector.LinkTargets(ReportRenderer.RenderReport(model));

        targets.Should().Contain(
        [
            "rules-overview",
            "rule-category-Users",
            "rule-category-Pats",
            "rule-category-Projects",
            "users-overview",
            "user-user-1",
            "user-user-2",
            "pats-overview",
            "pat-pat-1",
            "pat-pat-2",
        ]);

        // Rule ids are GUIDs and some destination names are cut by the PDF writer, so
        // match on a prefix that is still unique.
        foreach (var rule in model.RulesOverview.Rules)
        {
            var prefix = $"rule-{rule.Id.ToString()[..8]}";
            targets.Any(t => t.StartsWith(prefix, StringComparison.Ordinal)).Should().BeTrue($"rule '{rule.Name}' has its own section");
        }
    }

    [Test]
    public void FooterLinksToProductAndCompanyWebsites()
    {
        var uris = PdfInspector.Uris(ReportRenderer.RenderReport(ReportTestData.CreateEmptyModel()));

        uris.Should().Contain("https://ado-auditor.com");
        uris.Should().Contain("https://it-titans-gmbh.com");
    }

    [Test]
    public void ReportWithoutAnyFindingsStillRendersEveryFixedSection()
    {
        var pdf = ReportRenderer.RenderReport(ReportTestData.CreateEmptyModel());

        PdfInspector.LooksLikePdf(pdf).Should().BeTrue();
        PdfInspector.PageCount(pdf).Should().BeGreaterThanOrEqualTo(7);
        PdfInspector.LinkTargets(pdf).Should().Contain(["rules-overview", "users-overview", "pats-overview"]);
        PdfInspector.LinkTargets(pdf).Should().NotContain(t => t.StartsWith("rule-category-"));
    }

    [Test]
    public void ACategoryWithoutRulesGetsNoSection()
    {
        var model = ReportTestData.CreateFullModel();
        var onlyUserRules = new RulesOverview
        {
            Rules = model.RulesOverview.Rules.Where(rule => rule.Scope == Categories.Users).ToArray(),
        };

        var targets = PdfInspector.LinkTargets(ReportRenderer.RenderReport(WithRules(model, onlyUserRules)));

        targets.Should().Contain("rule-category-Users");
        targets.Should().NotContain("rule-category-Pats");
        targets.Should().NotContain("rule-category-Projects");
    }

    [Test]
    public void MoreFindingsProduceMoreRulePages()
    {
        var small = ReportRenderer.RenderReport(ReportTestData.CreateFullModel());

        var model = ReportTestData.CreateFullModel();
        var manyRules = Enumerable
            .Range(0, 60)
            .Select(i => ReportTestData.Rule($"Generated rule {i}", (Severities)(i % 3), (Categories)(i % 3), i))
            .ToArray();
        var large = ReportRenderer.RenderReport(WithRules(model, new RulesOverview { Rules = manyRules }));

        PdfInspector.PageCount(large).Should().BeGreaterThan(PdfInspector.PageCount(small));
    }

    [Test]
    public void LongWrappingTextIsLaidOutOverSeveralPagesInsteadOfFailing()
    {
        var sentence = "Personal access tokens with broad scopes can be used to read and change source code across every project. ";
        var longText = string.Concat(Enumerable.Repeat(sentence, 120));
        var model = ReportTestData.CreateFullModel();
        var rules = new RulesOverview
        {
            Rules = [ReportTestData.Rule("Verbose rule", Severities.Critical, description: longText, recommendation: longText)],
        };

        var pdf = ReportRenderer.RenderReport(WithRules(model, rules));

        PdfInspector.LooksLikePdf(pdf).Should().BeTrue();
        var shortRules = new RulesOverview { Rules = [ReportTestData.Rule("Verbose rule", Severities.Critical)] };
        var shortPdf = ReportRenderer.RenderReport(WithRules(model, shortRules));
        PdfInspector.PageCount(pdf).Should().BeGreaterThan(PdfInspector.PageCount(shortPdf));
    }

    [Test]
    public void LongNamesDoNotBreakTheTableOfContentsOrTables()
    {
        var model = ReportTestData.CreateFullModel();
        var name = new string('W', 90) + " " + new string('x', 200);
        var rules = new RulesOverview { Rules = [ReportTestData.Rule(name, Severities.Warning, Categories.Pats)] };

        var pdf = ReportRenderer.RenderReport(WithRules(model, rules));

        PdfInspector.LooksLikePdf(pdf).Should().BeTrue();
    }

    [Test]
    public void ReportOnlyCoversTheOrganizationItWasBuiltFor()
    {
        var first = ReportRenderer.RenderReport(ReportTestData.CreateFullModel("Contoso"));
        var second = ReportRenderer.RenderReport(ReportTestData.CreateFullModel("Fabrikam"));

        PdfInspector.Title(first).Should().Contain("Contoso").And.NotContain("Fabrikam");
        PdfInspector.Title(second).Should().Contain("Fabrikam").And.NotContain("Contoso");
    }

    private static AuditPdfModel WithRules(AuditPdfModel model, RulesOverview rules)
        => new()
        {
            Audit = model.Audit,
            Organization = model.Organization,
            RulesOverview = rules,
            UsersOverview = model.UsersOverview,
            UsersById = model.UsersById,
            PatsOverview = model.PatsOverview,
            PatsById = model.PatsById,
        };
}
