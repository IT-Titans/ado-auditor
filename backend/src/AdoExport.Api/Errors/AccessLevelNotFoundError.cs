using FluentResults;

namespace AdoExport.Api.Errors;

/// <summary>
/// Describes that an access level could not be found.
/// </summary>
public class AccessLevelNotFoundError : Error
{
    public AccessLevelNotFoundError(string userId)
    {
        Metadata.Add(nameof(userId), userId);
    }
}