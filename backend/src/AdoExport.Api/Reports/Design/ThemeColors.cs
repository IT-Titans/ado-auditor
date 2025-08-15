using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Design;

public static class ThemeColors
{
    /// <summary>
    /// Gets the default color used for texts.
    /// </summary>
    public static Color Default => Colors.Black;

    /// <summary>
    /// Gets the accent color that is in line with the accent color used in the frontend.
    /// </summary>
    public static Color Accent => Color.FromRGB(173, 70, 225);

    /// <summary>
    /// Gets the accent color that is in line with the accent color used in the frontend.
    /// </summary>
    public static Color ItTitansBlue => Color.FromRGB(0, 154, 226);

    /// <summary>
    /// Gets the link color.
    /// </summary>
    public static Color Link => Accent;

    public static class SeverityColors
    {
        /// <summary>
        /// Gets the foreground color for critical items.
        /// </summary>
        public static Color CriticalSeverityForeground => Colors.White;

        /// <summary>
        /// Gets the background color for critical items.
        /// </summary>
        public static Color CriticalSeverityBackground => Color.FromRGB(231, 0, 11);

        /// <summary>
        /// Gets the foreground color for warning severity items.
        /// </summary>
        public static Color WarningSeverityForeground => Colors.White;

        /// <summary>
        /// Gets the background color for warning severity items.
        /// </summary>
        public static Color WarningSeverityBackground => Color.FromRGB(225, 113, 11);

        /// <summary>
        /// Gets the foreground color for ok severity items.
        /// </summary>
        public static Color OkSeverityForeground => Colors.White;

        /// <summary>
        /// Gets the background color for ok severity items.
        /// </summary>
        public static Color OkSeverityBackground => Color.FromRGB(94, 165, 0);

        /// <summary>
        /// Gets the foreground color for "disabled" items.
        /// </summary>
        public static Color DisabledSeverityForeground => Colors.White;

        /// <summary>
        /// Gets the background color for "disabled" items.
        /// </summary>
        public static Color DisabledSeverityBackground => Color.FromRGB(166, 160, 155);
    }
}