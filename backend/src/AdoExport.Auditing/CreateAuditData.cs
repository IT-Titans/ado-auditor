using AdoExport.Client.AdoApi;

namespace AdoExport.Auditing;

public sealed class CreateAuditData
{
    /// <summary>
    /// Gets metadata about the audit.
    /// </summary>
    public required AuditMetadata Metadata { get; init; }

    /// <summary>
    /// Gets or sets the raw data based on which an audit can be created.
    /// </summary>
    public RawAuditData? ExportedData { get; set; }
}