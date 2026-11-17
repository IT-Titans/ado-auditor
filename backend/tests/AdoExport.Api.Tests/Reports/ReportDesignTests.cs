using System.Globalization;

using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Reports.Design;
using AdoExport.Api.Reports.Extensions;
using AdoExport.Api.Reports.Templates.Components;

using QuestPDF.Infrastructure;

namespace AdoExport.Api.Tests.Reports;

[TestFixture]
public class ReportDesignTests
{
    private static readonly Severities[] AllSeverities = [Severities.Critical, Severities.Warning, Severities.Ok];

    [Test]
    public void ToHexColorWritesUpperCaseRrggbb()
    {
        Color.FromRGB(0x0A, 0xBC, 0xDE).ToHexColor().Should().Be("#0ABCDE");
    }

    [Test]
    public void EverySeverityHasItsOwnBackgroundColor()
    {
        AllSeverities.Select(s => s.ToBackgroundColor()).Distinct().Should().HaveCount(3);
    }

    [Test]
    public void IconIsAnSvgDrawnInTheSeverityColor()
    {
        foreach (var severity in AllSeverities)
        {
            var icon = severity.ToIcon();

            icon.Should().StartWith("<svg").And.Contain(severity.ToBackgroundColor().ToHexColor());
        }
    }

    [Test]
    public void EverySeverityHasADifferentIcon()
    {
        AllSeverities.Select(s => s.ToIcon()).Distinct().Should().HaveCount(3);
    }

    [Test]
    public void ColorMappingsRejectSeveritiesThatDoNotExist()
    {
        var unknown = (Severities)99;

        Assert.Throws<ArgumentOutOfRangeException>(() => unknown.ToBackgroundColor());
        Assert.Throws<ArgumentOutOfRangeException>(() => unknown.ToForegroundColor());
        Assert.Throws<ArgumentOutOfRangeException>(() => unknown.ToIcon());
    }

    [Test]
    public void DatesAreShownInTheGeneralShortFormatOfTheCurrentCulture()
    {
        var date = new DateTime(2026, 3, 14, 9, 30, 0, DateTimeKind.Utc);

        date.FormatToDateWithTime().Should().Be(date.ToLocalTime().ToString("g", CultureInfo.CurrentCulture));
    }

    [Test]
    public void SectionIdsAreStableAndDistinctPerEntity()
    {
        var id = Guid.NewGuid().ToString();

        Sections.RuleSectionId(id).Should().Be($"rule-{id}");
        Sections.UserOverviewSectionId("7").Should().Be("user-7");
        Sections.PatByIdSectionId("9").Should().Be("pat-9");
        Sections.RuleCategoryId(Categories.Pats).Should().Be("rule-category-Pats");
        new[]
        {
            Sections.RulesOverviewSectionId(),
            Sections.UsersOverviewSectionId(),
            Sections.UsersByIdSectionId(),
            Sections.PatsOverviewSectionId(),
            Sections.PatsByIdSectionId(),
        }.Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void TypographyStylesCanBeRead()
    {
        new[]
        {
            Typography.HeaderTitle, Typography.SectionHeading, Typography.SectionSubHeading,
            Typography.SectionSubSubHeading, Typography.HyperLink, Typography.TableHeading,
            Typography.CoverTitle, Typography.CoverOrganization, Typography.CoverSubTitle,
            Typography.Headline, Typography.TextVerySmall,
        }.Should().NotContainNulls();
        ComponentStyles.Sections.PaddingBottom().Should().BeGreaterThan(0);
    }
}
