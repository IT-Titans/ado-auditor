using AdoExport.Api.Reports.Design;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components;

public class SectionHeadingComponent : IComponent
{
    private readonly string _title;

    private readonly Headings _heading;

    private readonly IEnumerable<SeverityCounts>? _severityCounts;

    public SectionHeadingComponent(string title, Headings heading, IEnumerable<SeverityCounts>? severityCounts = null)
    {
        Guard.IsNotNullOrWhiteSpace(title);

        _title = title;
        _heading = heading;
        _severityCounts = severityCounts;
    }

    public void Compose(IContainer container)
    {
        if (_severityCounts is null)
        {
            container
                .PaddingTop(8)
                .PaddingBottom(8)
                .Text(_title)
                .Style(GetTextStyle(_heading));
        }
        else
        {
            container
                .PaddingTop(8)
                .PaddingBottom(8)
                .Row(row =>
                {
                    row
                        .RelativeItem()
                        .Text(_title)
                        .Style(GetTextStyle(_heading));

                    foreach (var severityCount in _severityCounts)
                    {
                        row
                            .AutoItem()
                            .Component(new SeverityIconComponent(severityCount.Severity, severityCount.Count.ToString()));
                    }
                });
        }
    }

    private TextStyle GetTextStyle(Headings heading)
    {
        return heading switch
        {
            Headings.H1 => Typography.SectionHeading,
            Headings.H2 => Typography.SectionSubHeading,
            Headings.H3 => Typography.SectionSubSubHeading,
            _ => throw new ArgumentOutOfRangeException(nameof(heading), heading, null)
        };
    }
}