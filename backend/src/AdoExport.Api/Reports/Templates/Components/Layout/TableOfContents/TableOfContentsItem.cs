using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Layout.TableOfContents;

public class TableOfContentsItem : IComponent
{
    private readonly TableOfContentsItemModel _model;

    public TableOfContentsItem(TableOfContentsItemModel model)
    {
        Guard.IsNotNull(model);

        _model = model;
    }

    /// <inheritdoc />
    public void Compose(IContainer container)
    {
        container
            .SectionLink(_model.SectionId)
            .Row(row =>
            {
                row
                    .AutoItem()
                    .PaddingLeft(12 * _model.Indentation)
                    .AlignLeft()
                    .Text($"{_model.ItemNumber}.");

                row
                    .RelativeItem()
                    .PaddingLeft(4)
                    .AlignLeft()
                    .Text(_model.Name);

                row
                    .AutoItem()
                    .AlignRight()
                    .Text(text =>
                    {
                        text.BeginPageNumberOfSection(_model.SectionId);
                    });
            });
    }
}