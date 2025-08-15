using AdoExport.Api.Reports.Models;
using AdoExport.Api.Reports.Templates.Components;
using AdoExport.Api.Reports.Templates.Components.Layout;
using AdoExport.Api.Reports.Templates.Components.Layout.TableOfContents;
using AdoExport.Api.Reports.Templates.Components.PATs;
using AdoExport.Api.Reports.Templates.Components.Rules;
using AdoExport.Api.Reports.Templates.Components.Users;
using AdoExport.Common.Services;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates;

public class AuditPdfReportDocument : IDocument
{
    private readonly IDateTimeService _dateTimeService;

    public AuditPdfReportDocument(IDateTimeService dateTimeService, AuditPdfModel model)
    {
        Guard.IsNotNull(dateTimeService);
        Guard.IsNotNull(model);

        _dateTimeService = dateTimeService;

        Model = model;
    }

    private AuditPdfModel Model { get; }

    public DocumentMetadata GetMetadata()
    {
        var metadata = DocumentMetadata.Default;

        metadata.Author = "ADO Auditor";
        metadata.Creator = "ADO Auditor";
        metadata.Title = $"Security Audit {Model.Organization.Name} {Model.Audit.CreatedAt}";
        metadata.CreationDate = _dateTimeService.UtcNow();

        return metadata;
    }

    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                // General layout
                page.Margin(50);

                // Header
                page
                    .Header()
                    .SkipOnce()
                    .Component(new AuditHeaderComponent(Model));

                // Content
                page
                    .Content()
                    .Element(ComposeContent);

                // Footer
                page
                    .Footer()
                    .Component(new AuditFooterComponent(Model));
            });
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Component(new CoverPage(Model));
            column.Item().PageBreak();
            column.Item().Component(new TableOfContentsPage(Model));
            column.Item().PageBreak();
            column.Item().Component(new RulesOverviewSectionComponent(Sections.RulesOverviewSectionId(), Model.RulesOverview));
            column.Item().PageBreak();
            column.Item().Component(new UsersOverviewSectionComponent(Sections.UsersOverviewSectionId(), Model.UsersOverview));
            column.Item().PageBreak();
            column.Item().Component(new UsersByIdSectionComponent(Sections.UsersByIdSectionId(), Model.UsersById));
            column.Item().PageBreak();
            column.Item().Component(new PatsOverviewSectionComponent(Sections.PatsOverviewSectionId(), Model.PatsOverview));
            column.Item().PageBreak();
            column.Item().Component(new PatsByIdSectionComponent(Sections.PatsByIdSectionId(), Model.PatsById));
        });
    }
}