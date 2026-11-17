using AdoExport.Api.Controllers.Models;
using AdoExport.Auditing;

using Riok.Mapperly.Abstractions;

using DomainCategories = AdoExport.Auditing.Categories;
using DomainSeverities = AdoExport.Auditing.Severities;
using DtoCategories = AdoExport.Api.Controllers.Models.Categories;
using DtoSeverities = AdoExport.Api.Controllers.Models.Severities;

namespace AdoExport.Api.Controllers.Mappings;

/// <summary>
/// The generated implementation of <see cref="IAuditMapper"/>.
/// </summary>
/// <remarks>
/// Mapperly writes the bodies of the partial methods below at compile time, so a member
/// that no longer lines up is a build diagnostic rather than a runtime exception on a
/// request that happens to hit it.
/// <para>
/// Enums are matched by name, not by ordinal. The two enums currently declare their
/// members in the same order, which means a reordering would silently keep compiling
/// under the default by-value strategy while swapping the meaning of every value.
/// </para>
/// </remarks>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public partial class AuditMapper : IAuditMapper
{
    /// <inheritdoc />
    [MapperIgnoreSource(nameof(SummaryItem.Category))]
    public partial AuditSummaryCategoryItemDto MapSummaryItemToDto(SummaryItem item);

    /// <inheritdoc />
    public partial DtoSeverities MapSeverityToDto(DomainSeverities severity);

    /// <inheritdoc />
    public partial DomainSeverities MapSeverityToDomain(DtoSeverities severity);

    /// <inheritdoc />
    public partial DtoCategories MapCategoryToDto(DomainCategories category);
}
