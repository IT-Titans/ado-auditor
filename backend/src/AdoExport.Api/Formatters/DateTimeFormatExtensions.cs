using System.Globalization;

namespace AdoExport.Api.Formatters;

public static class DateTimeFormatExtensions
{
    /// <summary>
    /// Formats the given <paramref name="dateTime" /> in a unified format that can be processed by the frontend.
    /// </summary>
    public static string FormatForFrontend(this DateTime dateTime)
    {
        Guard.IsNotNull(dateTime);

        return dateTime.ToString("u", CultureInfo.InvariantCulture);
    }
}