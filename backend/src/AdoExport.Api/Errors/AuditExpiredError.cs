using FluentResults;

namespace AdoExport.Api.Errors;

/// <summary>
/// Describes that an audit expired.
/// </summary>
public class AuditExpiredError : Error
{
    public AuditExpiredError(string auditKey)
    {
        Metadata.Add(nameof(auditKey), auditKey);
    }
}