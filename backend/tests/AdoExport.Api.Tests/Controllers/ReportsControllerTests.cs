using System.Net;

using AdoExport.Api.Reports;
using AdoExport.Api.Tests.Controllers.Support;
using AdoExport.Common.Errors;

using FluentResults;

namespace AdoExport.Api.Tests.Controllers;

public class ReportsControllerTests : ControllerTestBase
{
    [Test]
    public async Task GetAuditPdfReport_ReturnsTheFileWithItsNameAndType()
    {
        var auditId = Guid.NewGuid();
        byte[] bytes = [0x25, 0x50, 0x44, 0x46];
        Factory.Pdf.Register(auditId, Result.Ok(new PdfFileInfo()
        {
            Bytes = bytes,
            MimeType = "application/pdf",
            FileName = "audit-report.pdf",
        }));
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/reports?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        response.Content.Headers.ContentDisposition?.FileName.Should().Be("audit-report.pdf");
        (await response.Content.ReadAsByteArrayAsync()).Should().Equal(bytes);
    }

    [Test]
    public async Task GetAuditPdfReport_WithoutAuditId_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync("/api/reports");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Audit-Id is required");
    }

    [Test]
    public async Task GetAuditPdfReport_ForExpiredAudit_ReturnsGone()
    {
        // Nothing registered for this id: the stub answers with an expired audit.
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/reports?auditId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Test]
    public async Task GetAuditPdfReport_WhenRenderingFails_ReturnsServerError()
    {
        var auditId = Guid.NewGuid();
        Factory.Pdf.Register(auditId, Result.Fail<PdfFileInfo>("renderer crashed"));
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/reports?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Test]
    public async Task GetAuditPdfReport_WhenCancelReportedWhileRequestIsStillOpen_IsTreatedAsUnhandledError()
    {
        var auditId = Guid.NewGuid();
        Factory.Pdf.Register(auditId, Result.Fail<PdfFileInfo>(new RequestWasCanceledError()));
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/reports?auditId={auditId}");

        // The exception middleware only swallows a cancellation when the caller really went
        // away (RequestAborted). Here the connection is open, so it is a server error.
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
