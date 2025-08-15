using AdoExport.Auditing;

using FluentResults;

namespace AdoExport.Api.Services;

public interface IAuditService
{
    /// <summary>
    /// Creates an audit.
    /// </summary>
    /// <param name="auditId">The audit id.</param>
    /// <param name="cancellationToken" />
    /// <returns>A result containing the audit.</returns>
    Task<Result<Audit>> CreateAuditAsync(Guid auditId, CancellationToken cancellationToken);
}