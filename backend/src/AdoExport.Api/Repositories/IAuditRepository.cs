using AdoExport.Api.Auditing;
using AdoExport.Auditing;

using FluentResults;

namespace AdoExport.Api.Repositories;

public interface IAuditRepository
{
    /// <summary>
    /// Gets an audit by its <paramref name="auditKey" />.
    /// </summary>
    /// <param name="auditKey">The audit key.</param>
    /// <returns>A result whose value is the audit identified by <paramref name="auditKey" />.</returns>
    Task<Result<Audit>> GetAuditByIdAsync(string auditKey);

    /// <summary>
    /// Gets an audit by its <paramref name="auditId" />.
    /// </summary>
    /// <param name="auditId">The audit id.</param>
    /// <returns>A result whose value is the audit identified by <paramref name="auditId" />.</returns>
    Task<Result<Audit>> GetAuditByIdAsync(Guid auditId);
}