using System.Net.Mime;

using AdoExport.Api.Controllers.Extensions;
using AdoExport.Api.Errors;
using AdoExport.Api.Reports;
using AdoExport.Api.Services;
using AdoExport.Common.Errors;

using Microsoft.AspNetCore.Mvc;

namespace AdoExport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ILogger<ReportsController> _logger;

    private readonly IPdfReportService _pdfReportService;

    public ReportsController(
        ILogger<ReportsController> logger,
        IPdfReportService pdfReportService)
    {
        Guard.IsNotNull(logger);
        Guard.IsNotNull(pdfReportService);

        _logger = logger;
        _pdfReportService = pdfReportService;
    }

    [HttpGet(Name = "GetAuditPdfReport")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [Produces(MediaTypeNames.Application.Pdf)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status410Gone)]
    public async Task<IActionResult> GetAuditPdfReport(
        [FromQuery] Guid auditId,
        CancellationToken cancellationToken,
        [FromServices] ScopedAuditKeyBearer auditKeyBearer)
    {
        if (auditId == Guid.Empty)
        {
            return BadRequest("Audit-Id is required");
        }

        var auditKey = auditId.ToString();
        return await auditKeyBearer.ScopedAsync<IActionResult>(auditKey, async () =>
        {
            var pdfResult = await _pdfReportService.GetAuditReportPdfFileAsync(auditId, cancellationToken).ConfigureAwait(false);
            if (pdfResult.IsFailed)
            {
                if (pdfResult.RequestWasCanceled())
                {
                    throw new OperationCanceledException(cancellationToken);
                }

                if (pdfResult.HasError<AuditExpiredError>())
                {
                    return this.AuditExpiredStatusCode();
                }

                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError("Failed to generate pdf report. {Result}", pdfResult);
                }

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var pdfFileInfo = pdfResult.Value;

            return File(pdfFileInfo.Bytes, pdfFileInfo.MimeType, pdfFileInfo.FileName);
        }).ConfigureAwait(false);
    }
}