using AdoExport.Api.Reports.Extensions;

using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Design;

public static class Icons
{
    /// <summary>
    /// Gets the "critical" icon as colored svg.<br />
    /// Source: https://icones.js.org/collection/tabler
    /// </summary>
    /// <param name="color">The icon color.</param>
    /// <returns>A string containing the svg data.</returns>
    public static string CriticalIcon(Color color)
    {
        Guard.IsNotNull(color);

        return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"32\" height=\"32\" viewBox=\"0 0 24 24\"><!-- Icon from Tabler Icons by Paweł Kuna - https://github.com/tabler/tabler-icons/blob/master/LICENSE --><path fill=\"none\" stroke=\"{color.ToHexColor()}\" stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M12 10.941c2.333-3.308.167-7.823-1-8.941c0 3.395-2.235 5.299-3.667 6.706C5.903 10.114 5 12.327 5 14.294C5 17.998 8.134 21 12 21s7-3.002 7-6.706c0-1.712-1.232-4.403-2.333-5.588c-2.084 3.353-3.257 3.353-4.667 2.235\"/></svg>";
    }

    /// <summary>
    /// Gets the "warning" icon as colored svg.<br />
    /// Source: https://icones.js.org/collection/tabler
    /// </summary>
    /// <param name="color">The icon color.</param>
    /// <returns>A string containing the svg data.</returns>
    public static string WarningIcon(Color color)
    {
        Guard.IsNotNull(color);

        return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"32\" height=\"32\" viewBox=\"0 0 24 24\"><!-- Icon from Tabler Icons by Paweł Kuna - https://github.com/tabler/tabler-icons/blob/master/LICENSE --><path fill=\"none\" stroke=\"{color.ToHexColor()}\" stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M3 12a9 9 0 1 0 18 0a9 9 0 0 0-18 0m9-4v4m0 4h.01\"/></svg>";
    }

    /// <summary>
    /// Gets the "olk" icon as colored svg.<br />
    /// Source: https://icones.js.org/collection/tabler
    /// </summary>
    /// <param name="color">The icon color.</param>
    /// <returns>A string containing the svg data.</returns>
    public static string OkIcon(Color color)
    {
        Guard.IsNotNull(color);

        return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"32\" height=\"32\" viewBox=\"0 0 24 24\"><!-- Icon from Tabler Icons by Paweł Kuna - https://github.com/tabler/tabler-icons/blob/master/LICENSE --><g fill=\"none\" stroke=\"{color.ToHexColor()}\" stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\"><path d=\"m9 12l2 2l4-4\"/><path d=\"M12 3c7.2 0 9 1.8 9 9s-1.8 9-9 9s-9-1.8-9-9s1.8-9 9-9\"/></g></svg>";
    }
}