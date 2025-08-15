using AdoExport.Api.Reports.Design;
using AdoExport.Api.Reports.Models;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Layout;

public class AuditFooterComponent : IComponent
{
    private readonly AuditPdfModel _model;

    public AuditFooterComponent(AuditPdfModel model)
    {
        Guard.IsNotNull(model);

        _model = model;
    }

    public void Compose(IContainer container)
    {
        container
            .PaddingTop(20)
            .Column(column =>
            {
                column
                    .Item()
                    .Row(row =>
                    {
                        row
                            .RelativeItem()
                            .Text(text =>
                            {
                                text
                                    .Hyperlink(Properties.Reports.AdoProductName, Properties.Reports.AuditorWebsiteUrl)
                                    .Style(Typography.HyperLink);

                                text.Span(" ");
                                text.Span(Properties.Reports.Cover_AProductBy);
                                text.Span(" ");

                                text
                                    .Hyperlink(Properties.Reports.ItTitans_CompanyName, Properties.Reports.ItTitans_WebsiteUrl)
                                    .Style(Typography
                                        .HyperLink
                                        .FontColor(ThemeColors.ItTitansBlue));
                            });

                        row
                            .ConstantItem(40)
                            .AlignRight()
                            .Text(text => text.CurrentPageNumber());
                    });

                column
                    .Item()
                    .PaddingTop(4)
                    .Text(text =>
                    {
                        text
                            .Span(Properties.Reports.Header_AuditedAt)
                            .Style(Typography.TextVerySmall)
                            .SemiBold();
                        text
                            .Span(" ")
                            .Style(Typography.TextVerySmall);
                        text
                            .Span(_model.Audit.CreatedAt.FormatToDateWithTime())
                            .Style(Typography.TextVerySmall);
                    });
            });
    }
}