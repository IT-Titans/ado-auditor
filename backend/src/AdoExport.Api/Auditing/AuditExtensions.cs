using AdoExport.Api.Formatters;
using AdoExport.Auditing;

namespace AdoExport.Api.Auditing;

public static class AuditExtensions
{
    /// <summary>
    /// Formats the <see cref="AuditMetadata.RequestedAuditAt" /> date into a unified format.
    /// </summary>
    public static string RequestedAtFormatted(this Audit audit)
    {
        Guard.IsNotNull(audit);

        return audit.Metadata.RequestedAuditAt.FormatForFrontend();
    }

    /// <summary>
    /// Formats the <see cref="AuditMetadata.CreatedAuditAt" /> date into a unified format.
    /// </summary>
    public static string CreatedAtFormatted(this Audit audit)
    {
        Guard.IsNotNull(audit);

        return audit.Metadata.CreatedAuditAt.FormatForFrontend();
    }

    /// <summary>
    /// Calculates the timestamp the audit is valid upon.
    /// </summary>
    public static DateTime ValidUntil(this Audit audit)
    {
        Guard.IsNotNull(audit);

        return audit.Metadata.CreatedAuditAt.Add(audit.Metadata.ExpiresAfter);
    }

    /// <summary>
    /// Formats the <see cref="ValidUntil" /> date into a unified format.
    /// </summary>
    public static string ValidUntilFormatted(this Audit audit)
    {
        Guard.IsNotNull(audit);

        return audit.ValidUntil().FormatForFrontend();
    }
}