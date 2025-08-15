using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Design;

public static class Typography
{
    /// <summary>
    /// Gets the default style for a content section heading.
    /// </summary>
    public static TextStyle HeaderTitle => TextStyle
        .Default
        .FontColor(ThemeColors.Accent)
        .FontSize(20)
        .SemiBold();

    /// <summary>
    /// Gets the default style for a content section heading.
    /// </summary>
    public static TextStyle SectionHeading => TextStyle
        .Default
        .FontColor(ThemeColors.Default)
        .FontSize(18)
        .Bold();

    /// <summary>
    /// Gets the default style for a content section sub heading.
    /// </summary>
    public static TextStyle SectionSubHeading => Typography
        .SectionHeading
        .FontSize(16);

    /// <summary>
    /// Gets the default style for a content section sub sub heading.
    /// </summary>
    public static TextStyle SectionSubSubHeading => Typography
        .SectionHeading
        .FontSize(14);

    /// <summary>
    /// Gets the default style for a hyperlink.
    /// </summary>
    public static TextStyle HyperLink => TextStyle
        .Default
        .FontColor(ThemeColors.Link)
        .Underline();

    /// <summary>
    /// Gets the default style for a content section heading.
    /// </summary>
    public static TextStyle TableHeading => TextStyle
        .Default
        .FontColor(ThemeColors.Default)
        .FontSize(12)
        .Bold();

    /// <summary>
    /// Gets the default style for a content section heading.
    /// </summary>
    public static TextStyle CoverTitle => TextStyle
        .Default
        .FontColor(ThemeColors.Default)
        .FontSize(22)
        .SemiBold();

    /// <summary>
    /// Gets the default style for a content section heading.
    /// </summary>
    public static TextStyle CoverOrganization => TextStyle
        .Default
        .FontColor(ThemeColors.Accent)
        .FontSize(30)
        .SemiBold();

    /// <summary>
    /// Gets the default style for a content section heading.
    /// </summary>
    public static TextStyle CoverSubTitle => TextStyle
        .Default
        .FontColor(ThemeColors.Default)
        .FontSize(16)
        .SemiBold();

    public static TextStyle Headline => TextStyle
        .Default
        .FontSize(16)
        .SemiBold();

    public static TextStyle TextVerySmall => TextStyle
        .Default
        .FontSize(9);
}