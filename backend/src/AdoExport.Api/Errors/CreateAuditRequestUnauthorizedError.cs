using FluentResults;

namespace AdoExport.Api.Errors;

/// <summary>
/// Describes that a request to create an audit was not authorized.
/// </summary>
public class CreateAuditRequestUnauthorizedError : Error
{
    public CreateAuditRequestUnauthorizedError(Guid auditId)
    {
        Metadata[nameof(auditId)] = auditId;
    }
}