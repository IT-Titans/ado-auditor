using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Reports.Design;
using AdoExport.Api.Reports.Extensions;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components;

public class AffectedCountComponent : IComponent
{
    private readonly Severities _severity;

    private readonly int _count;

    public AffectedCountComponent(Severities severity, int count)
    {
        _severity = severity;
        _count = count;
    }

    public void Compose(IContainer container)
    {
        var backgroundColor = _count == 0
            ? ThemeColors.SeverityColors.DisabledSeverityBackground
            : _severity.ToBackgroundColor();

        var foregroundColor = _count == 0
            ? ThemeColors.SeverityColors.DisabledSeverityForeground
            : _severity.ToForegroundColor();

        container
            .Width(25)
            .Height(25)
            .Background(backgroundColor)
            .CornerRadius(15)
            .Padding(5)
            .AlignCenter()
            .AlignMiddle()
            .Text(_count.ToString())
            .FontColor(foregroundColor);
    }
}