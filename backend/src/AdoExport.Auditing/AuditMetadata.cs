namespace AdoExport.Auditing;

public sealed class AuditMetadata
{
    /// <summary>
    /// Gets or sets the audit id.
    /// </summary>
    public required Guid AuditId { get; init; }

    /// <summary>
    /// Gets or sets the name of the organization the audit should be created for.
    /// </summary>
    public required string OrganizationName { get; init; }

    /// <summary>
    /// Gets or sets the duration after which the audit expires.
    /// </summary>
    public required TimeSpan ExpiresAfter { get; init; }

    /// <summary>
    /// Gets or sets a timestamp for when the audit was first requested.
    /// </summary>
    public DateTime RequestedAuditAt { get; set; }

    /// <summary>
    /// Gets or sets a timestamp for when retrieving data for the audit was started.<br />
    /// This happens after <see cref="RequestedAuditAt" />.
    /// </summary>
    public DateTime StartRequestAuditDataAt { get; set; }

    /// <summary>
    /// Gets or sets a timestamp for when retrieving data for the audit was finished.<br />
    /// This happens after <see cref="StartRequestAuditDataAt" />.
    /// </summary>
    public DateTime? FinishRequestAuditDataAt { get; set; }

    /// <summary>
    /// Gets or sets a timestamp for when calculating the audit was started.<br />
    /// This happens after <see cref="FinishRequestAuditDataAt" />.
    /// </summary>
    public DateTime? StartCalculatingAuditAt { get; set; }

    /// <summary>
    /// Gets or sets a timestamp for when calculating the audit was finished.<br />
    /// This happens after <see cref="StartCalculatingAuditAt" />.
    /// </summary>
    public DateTime? FinishCalculatingAuditAt { get; set; }

    /// <summary>
    /// Gets or sets a timestamp for when the audit was first created.<br />
    /// This happens after <see cref="FinishCalculatingAuditAt" />.
    /// </summary>
    public DateTime CreatedAuditAt { get; set; }
}