namespace AdoExport.Api.Reports.Templates.Components.Layout.TableOfContents;

public sealed class TableOfContentsItemModel
{
    public required string SectionId { get; init; }
    
    public required int  Indentation { get; init; }
    
    public required int  ItemNumber { get; init; }

    public required string  Name { get; init; }
}