using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing;

public class Audit
{
    public Audit(AuditMetadata metadata)
    {
        Guard.IsNotNull(metadata);

        AuditId = metadata.AuditId;
        Metadata = metadata;
    }

    /// <summary>
    /// Gets or sets the audit id. 
    /// </summary>
    public Guid AuditId { get; private init; }

    /// <summary>
    /// Gets or sets the audits metadata.
    /// </summary>
    public string OrganizationName => Metadata.OrganizationName;

    /// <summary>
    /// Gets or sets the audits metadata.
    /// </summary>
    public AuditMetadata Metadata { get; private init; }

    public required AuditResult<SessionTokenDto>[] PatAuditResults { get; init; }

    public required AuditResult<SessionTokenDto>[] SshKeysAuditResults { get; init; }

    public required AuditResult<UserDto>[] UserAuditResults { get; init; }
}