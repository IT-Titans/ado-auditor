using AdoExport.Api.Reports.Design;
using AdoExport.Api.Reports.Models;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Layout;

public class AuditHeaderComponent : IComponent
{
    private readonly AuditPdfModel _model;

    public AuditHeaderComponent(AuditPdfModel model)
    {
        _model = model;
    }

    public void Compose(IContainer container)
    {
        container
            .PaddingBottom(20)
            .Row(row =>
            {
                row
                    .RelativeItem()
                    .AlignLeft()
                    .Text($"{Properties.Reports.Header_AuditForOrganization} {_model.Organization.Name}")
                    .Style(Typography.HeaderTitle);

                row
                    .ConstantItem(110)
                    .AlignRight()
                    .Height(25)
                    .Width(100)
                    .Image(Properties.Reports.ADO_Logo);
            });
    }
}