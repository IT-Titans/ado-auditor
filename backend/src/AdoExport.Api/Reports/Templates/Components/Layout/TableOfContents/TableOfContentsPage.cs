using AdoExport.Api.Reports.Models;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Layout.TableOfContents;

public class TableOfContentsPage : IComponent
{
    private readonly AuditPdfModel _model;

    public TableOfContentsPage(AuditPdfModel model)
    {
        Guard.IsNotNull(model);

        _model = model;
    }

    public void Compose(IContainer container)
    {
        container.Column(column =>
        {
            column
                .Item()
                .Extend()
                .Component(new TableOfContentsComponent(_model));
        });
    }
}