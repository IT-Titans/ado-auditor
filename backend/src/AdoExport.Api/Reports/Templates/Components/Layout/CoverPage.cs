using AdoExport.Api.Reports.Models;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Layout;

public class CoverPage : IComponent
{
    private readonly AuditPdfModel _model;

    public CoverPage(AuditPdfModel model)
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
                .AlignMiddle()
                .AlignCenter()
                .Component(new CoverComponent(_model));
        });
    }
}