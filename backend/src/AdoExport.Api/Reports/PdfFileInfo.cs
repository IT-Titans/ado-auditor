namespace AdoExport.Api.Reports;

public class PdfFileInfo
{
    /// <summary>
    /// Gets the raw bytes the file consists of.
    /// </summary>
    public required byte[] Bytes { get; init; }

    /// <summary>
    /// Gets the pdf mime type.
    /// </summary>
    public required string MimeType { get; init; }

    /// <summary>
    /// Gets the file name that is used when the file gets downloaded.
    /// </summary>
    public required string FileName { get; init; }
}