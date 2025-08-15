namespace AdoExport.Api.Reports.Design;

public static class Formatters
{
    /// <summary>
    /// Formats the given <paramref name="dateTime" /> to a localized, displayable string.<br />
    /// The format is: 2009-06-15T13:45:30 -> 6/15/2009 1:45:30 PM
    /// </summary>
    /// <param name="dateTime">The date time to format.</param>
    /// <returns>A displayable string.</returns>
    public static string FormatToDateWithTime(this DateTime dateTime) => dateTime.ToLocalTime().ToString("g");
}