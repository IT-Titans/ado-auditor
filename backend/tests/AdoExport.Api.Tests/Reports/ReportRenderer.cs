using AdoExport.Api.Reports.Models;
using AdoExport.Api.Reports.Templates;
using AdoExport.Common.Services;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Tests.Reports;

internal static class ReportRenderer
{
    public static readonly DateTime FixedNow = new(2026, 3, 14, 12, 0, 0, DateTimeKind.Utc);

    static ReportRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static IDateTimeService FixedClock()
    {
        var clock = new Mock<IDateTimeService>();
        clock.Setup(c => c.UtcNow()).Returns(FixedNow);
        return clock.Object;
    }

    public static AuditPdfReportDocument CreateDocument(AuditPdfModel model)
        => new(FixedClock(), model);

    public static byte[] RenderReport(AuditPdfModel model)
        => CreateDocument(model).GeneratePdf();

    /// <summary>
    /// Renders a single component on its own A4 page, which exercises its branches without
    /// the rest of the report around it.
    /// </summary>
    public static byte[] RenderComponent(IComponent component)
        => Document
            .Create(document => document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.Content().Component(component);
            }))
            .GeneratePdf();
}
