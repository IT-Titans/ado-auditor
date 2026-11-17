using AdoExport.Api.Controllers.Mappings;
using AdoExport.Auditing;

using DomainCategories = AdoExport.Auditing.Categories;
using DomainSeverities = AdoExport.Auditing.Severities;
using DtoCategories = AdoExport.Api.Controllers.Models.Categories;
using DtoSeverities = AdoExport.Api.Controllers.Models.Severities;

namespace AdoExport.Api.Tests;

/// <summary>
/// Replaces the former AutoMapper configuration test.
/// </summary>
/// <remarks>
/// That test asserted only that a configuration existed and was internally consistent.
/// These cases assert the far more useful property: that every member of the domain
/// enums has a counterpart on the DTO side and arrives as the member of the same name.
/// A member added on one side only is what would actually break a response, and it is
/// caught here rather than on the request that happens to produce it.
/// </remarks>
public class AuditMapperTests
{
    private readonly IAuditMapper _mapper = new AuditMapper();

    [Test]
    public void MapsEverySeverityToTheMemberOfTheSameName()
    {
        foreach (var severity in Enum.GetValues<DomainSeverities>())
        {
            _mapper
                .MapSeverityToDto(severity)
                .ToString()
                .Should()
                .Be(severity.ToString());
        }
    }

    [Test]
    public void MapsEverySeverityBackFromTheDtoSide()
    {
        foreach (var severity in Enum.GetValues<DtoSeverities>())
        {
            _mapper
                .MapSeverityToDomain(severity)
                .ToString()
                .Should()
                .Be(severity.ToString());
        }
    }

    [Test]
    public void RoundTripsEverySeverityWithoutLoss()
    {
        foreach (var severity in Enum.GetValues<DomainSeverities>())
        {
            _mapper
                .MapSeverityToDomain(_mapper.MapSeverityToDto(severity))
                .Should()
                .Be(severity);
        }
    }

    [Test]
    public void MapsEveryCategoryToTheMemberOfTheSameName()
    {
        foreach (var category in Enum.GetValues<DomainCategories>())
        {
            _mapper
                .MapCategoryToDto(category)
                .ToString()
                .Should()
                .Be(category.ToString());
        }
    }

    [Test]
    public void CoversEveryDtoCategory()
    {
        // Guards the other direction: a category that only the DTO layer knows would
        // leave MapCategoryToDto unable to ever produce it.
        var produced = Enum
            .GetValues<DomainCategories>()
            .Select(_mapper.MapCategoryToDto)
            .ToArray();

        produced
            .Should()
            .BeEquivalentTo(Enum.GetValues<DtoCategories>());
    }

    [Test]
    public void CopiesEveryScalarOfASummaryItem()
    {
        var item = new SummaryItem
        {
            Severity = DomainSeverities.Warning,
            Category = DomainCategories.Pats,
            AffectedCount = 3,
            TotalCount = 7,
            Text = "Personal access tokens without an expiry"
        };

        var dto = _mapper.MapSummaryItemToDto(item);

        dto.Severity.Should().Be(DtoSeverities.Warning);
        dto.AffectedCount.Should().Be(3);
        dto.TotalCount.Should().Be(7);
        dto.Text.Should().Be("Personal access tokens without an expiry");
    }
}
