using AdoExport.Api.Controllers.Models;
using AdoExport.Auditing;

using DomainCategories = AdoExport.Auditing.Categories;
using DomainSeverities = AdoExport.Auditing.Severities;
using DtoCategories = AdoExport.Api.Controllers.Models.Categories;
using DtoSeverities = AdoExport.Api.Controllers.Models.Severities;

namespace AdoExport.Api.Controllers.Mappings;

/// <summary>
/// Translates auditing domain types into the shapes the API returns, and back where a
/// request carries one of them as a filter.
/// </summary>
/// <remarks>
/// The domain and the DTO layer each own an enum of the same name. Every signature here
/// spells out which side it means, because <c>Severities</c> on its own is ambiguous in
/// any file that sees both namespaces.
/// </remarks>
public interface IAuditMapper
{
    /// <summary>
    /// Maps a single summary item onto its DTO. The category is carried by the enclosing
    /// <see cref="AuditSummaryCategoryDto"/> and is therefore not repeated on the item.
    /// </summary>
    AuditSummaryCategoryItemDto MapSummaryItemToDto(SummaryItem item);

    /// <summary>Maps a domain severity onto the value serialised to clients.</summary>
    DtoSeverities MapSeverityToDto(DomainSeverities severity);

    /// <summary>Maps a severity supplied by a client back onto the domain value.</summary>
    DomainSeverities MapSeverityToDomain(DtoSeverities severity);

    /// <summary>Maps a domain category onto the value serialised to clients.</summary>
    DtoCategories MapCategoryToDto(DomainCategories category);
}
