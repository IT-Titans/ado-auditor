using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace AdoExport.Api.Tests.Reports;

/// <summary>
/// Reads just enough of a PDF to make assertions about it without a PDF parsing package.
/// </summary>
/// <remarks>
/// Fonts are subset and page text is glyph encoded, so the text itself cannot be read back.
/// Dictionaries and annotations can: document title, page count, external links and the
/// named destinations that section links point to. Those are what is asserted on.
/// </remarks>
internal static partial class PdfInspector
{
    public static bool LooksLikePdf(byte[] bytes)
    {
        if (bytes.Length < 16)
        {
            return false;
        }

        var tailLength = Math.Min(bytes.Length, 32);
        var tail = Encoding.ASCII.GetString(bytes, bytes.Length - tailLength, tailLength);
        return Encoding.ASCII.GetString(bytes, 0, 5) == "%PDF-" && tail.Contains("%%EOF");
    }

    /// <summary>
    /// The page count the document declares in its page tree root.
    /// </summary>
    public static int PageCount(byte[] bytes)
    {
        var counts = PagesCountRegex()
            .Matches(Decoded(bytes))
            .Select(m => int.Parse(m.Groups[1].Value))
            .ToArray();
        return counts.Length == 0 ? 0 : counts.Max();
    }

    /// <summary>
    /// The document title from the info dictionary.
    /// </summary>
    public static string? Title(byte[] bytes)
    {
        var match = TitleRegex().Match(Decoded(bytes));
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// The section ids the document links to. Every section link in the report becomes a
    /// named destination, so this shows which sections were rendered and linked.
    /// </summary>
    public static IReadOnlySet<string> LinkTargets(byte[] bytes)
        => DestRegex()
            .Matches(Decoded(bytes))
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

    /// <summary>
    /// The external URIs the document links to.
    /// </summary>
    public static IReadOnlySet<string> Uris(byte[] bytes)
        => UriRegex()
            .Matches(Decoded(bytes))
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

    /// <summary>
    /// The raw file text followed by the inflated content of every Flate stream.
    /// </summary>
    public static string Decoded(byte[] bytes)
    {
        var raw = Encoding.Latin1.GetString(bytes);
        var builder = new StringBuilder(raw);

        foreach (System.Text.RegularExpressions.Match stream in StreamRegex().Matches(raw))
        {
            var start = stream.Index + stream.Length;
            var end = raw.IndexOf("endstream", start, StringComparison.Ordinal);
            if (end < 0)
            {
                continue;
            }

            try
            {
                using var input = new MemoryStream(bytes, start, end - start);
                using var inflate = new ZLibStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();
                inflate.CopyTo(output);
                builder.Append('\n').Append(Encoding.Latin1.GetString(output.ToArray()));
            }
            catch (InvalidDataException)
            {
                // Not a Flate stream (images, fonts); nothing to add.
            }
        }

        return builder.ToString();
    }

    [GeneratedRegex(@"/Type\s*/Pages[^>]*?/Count\s+(\d+)", RegexOptions.Singleline)]
    private static partial Regex PagesCountRegex();

    [GeneratedRegex(@"/Title\s*\(([^)]*)\)")]
    private static partial Regex TitleRegex();

    [GeneratedRegex(@"/Dest\s+/0#20\|#20([^\s/>\]]+)")]
    private static partial Regex DestRegex();

    [GeneratedRegex(@"/URI\s+\(([^)]*)\)")]
    private static partial Regex UriRegex();

    [GeneratedRegex(@"stream\r?\n")]
    private static partial Regex StreamRegex();
}
