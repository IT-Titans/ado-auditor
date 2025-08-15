using AdoExport.Api.Reports.Models;

using FluentResults;

namespace AdoExport.Api.Reports.DataSources;

public interface IAuditReportDataSource
{
    /// <summary>
    /// Gets the data required for the <see cref="AuditPdfModel" />.
    /// </summary>
    /// <param name="cancellationToken" />
    /// <returns>A result containing the <see cref="AuditPdfModel" />.</returns>
    Task<Result<AuditPdfModel>> GetAuditDataAsync(CancellationToken cancellationToken);
}