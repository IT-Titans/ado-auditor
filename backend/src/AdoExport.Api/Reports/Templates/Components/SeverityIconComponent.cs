using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Reports.Design;
using AdoExport.Api.Reports.Extensions;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components;

public class SeverityIconComponent : IComponent
{
    private readonly Severities _severity;

    private readonly string? _text;

    public SeverityIconComponent(Severities severity, string? text = null)
    {
        _severity = severity;
        _text = text;
    }

    public void Compose(IContainer container)
    {
        container
            .Row(row =>
            {
                row
                    .AutoItem()
                    .PaddingRight(4)
                    .Height(24)
                    .AlignMiddle()
                    .ShowIf(string.IsNullOrWhiteSpace(_text) == false)
                    .Text(_text);

                row
                    .AutoItem()
                    .Width(24)
                    .Height(24)
                    .Svg(_severity.ToIcon())
                    .FitArea();
            });
    }
}