using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Reports.Design;

using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Extensions;

public static class ColorExtensions
{
    /// <summary>
    /// Gets the hex representation of the given <paramref name="color" /> in the form of #RRGGBB.
    /// </summary>
    /// <param name="color">The color to get the hex representation for.</param>
    /// <returns>The given <paramref name="color" /> in hex format.</returns>
    public static string ToHexColor(this Color color)
    {
        Guard.IsNotNull(color);

        return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
    }

    public static Color ToBackgroundColor(this Severities severity)
    {
        return severity switch
        {
            Severities.Critical => ThemeColors.SeverityColors.CriticalSeverityBackground,
            Severities.Warning => ThemeColors.SeverityColors.WarningSeverityBackground,
            Severities.Ok => ThemeColors.SeverityColors.OkSeverityBackground,
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        };
    }

    public static Color ToForegroundColor(this Severities severity)
    {
        return severity switch
        {
            Severities.Critical => ThemeColors.SeverityColors.CriticalSeverityForeground,
            Severities.Warning => ThemeColors.SeverityColors.WarningSeverityForeground,
            Severities.Ok => ThemeColors.SeverityColors.OkSeverityForeground,
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        };
    }

    public static string ToIcon(this Severities severity)
    {
        var iconBackgroundColor = severity.ToBackgroundColor();
        return severity switch
        {
            Severities.Critical => Icons.CriticalIcon(iconBackgroundColor),
            Severities.Warning => Icons.WarningIcon(iconBackgroundColor),
            Severities.Ok => Icons.OkIcon(iconBackgroundColor),
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        };
    }
}