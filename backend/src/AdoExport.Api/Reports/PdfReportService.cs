using AdoExport.Api.Reports.DataSources;
using AdoExport.Api.Reports.Models;
using AdoExport.Api.Reports.Templates;
using AdoExport.Api.Services;
using AdoExport.Common.Errors;

using FluentResults;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports;

public class PdfReportService : IPdfReportService
{
    private readonly ScopedAuditKeyBearer _auditKeyBearer;

    private readonly IAuditReportDataSource _auditReportDataSource;

    private readonly Func<AuditPdfModel, AuditPdfReportDocument> _createReportDocument;

    public PdfReportService(
        ScopedAuditKeyBearer auditKeyBearer,
        IAuditReportDataSource auditReportDataSource,
        Func<AuditPdfModel, AuditPdfReportDocument> createReportDocument)
    {
        Guard.IsNotNull(auditKeyBearer);
        Guard.IsNotNull(auditReportDataSource);
        Guard.IsNotNull(createReportDocument);

        _auditKeyBearer = auditKeyBearer;
        _auditReportDataSource = auditReportDataSource;
        _createReportDocument = createReportDocument;

        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <inheritdoc />
    public async Task<Result<PdfFileInfo>> GetAuditReportPdfFileAsync(Guid auditId, CancellationToken cancellationToken)
    {
        Guard.IsNotDefault(auditId);

        var auditKey = auditId.ToString();
        return await _auditKeyBearer.ScopedAsync(auditKey, async () =>
        {
            var auditModelResult = await _auditReportDataSource.GetAuditDataAsync(cancellationToken).ConfigureAwait(false);
            if (auditModelResult.IsFailed)
            {
                return auditModelResult.ToResult();
            }

            var auditModel = auditModelResult.Value;

            if (cancellationToken.IsCancellationRequested)
            {
                return Result.Fail(new RequestWasCanceledError());
            }

            var pdfDocument = Document.Create(container => _createReportDocument(auditModel).Compose(container));

            return Result.Ok(new PdfFileInfo()
            {
                MimeType = System.Net.Mime.MediaTypeNames.Application.Pdf,
                FileName = $"{pdfDocument.GetMetadata().Title}.pdf",
                Bytes = pdfDocument.GeneratePdf(),
            });
        });
    }
}