using FluentResults;

namespace AdoExport.Api.Reports;

public interface IPdfReportService
{
    Task<Result<PdfFileInfo>> GetAuditReportPdfFileAsync(Guid auditId, CancellationToken cancellationToken);
}