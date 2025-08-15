using AdoExport.Api.Reports.Design;
using AdoExport.Api.Reports.Models;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Layout;

public class CoverComponent : IComponent
{
    private readonly AuditPdfModel _model;

    public CoverComponent(AuditPdfModel model)
    {
        Guard.IsNotNull(model);

        _model = model;
    }

    public void Compose(IContainer container)
    {
        container
            .Column(column =>
            {
                column
                    .Item()
                    .PaddingBottom(20)
                    .Text(Properties.Reports.Cover_Title)
                    .Style(Typography.CoverTitle);

                column
                    .Item()
                    .Text(_model.Organization.Name)
                    .Style(Typography.CoverOrganization);

                column
                    .Item()
                    .PaddingTop(15)
                    .Text(text =>
                    {
                        text.Span(Properties.Reports.Cover_CreatedAt).Style(Typography.CoverSubTitle);
                        text.Span(" ");
                        text.Span(_model.Audit.CreatedAt.FormatToDateWithTime()).Style(Typography.CoverSubTitle);
                    });
            });

    }
}