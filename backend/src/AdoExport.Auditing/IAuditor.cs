using FluentResults;

namespace AdoExport.Auditing;

public interface IAuditor
{
    /// <summary>
    /// Creates an <see cref="Audit" /> for the data associated with <paramref name="createAuditData" />.
    /// </summary>
    Task<Result<Audit>> AuditOrganizationAsync(CreateAuditData createAuditData);
}