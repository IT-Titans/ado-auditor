using FluentResults;

namespace AdoExport.Api.Errors;

/// <summary>
/// Describes that a user could not be found.
/// </summary>
public class UserNotFoundError : Error
{
    public UserNotFoundError(string userId)
    {
        Metadata.Add(nameof(userId), userId);
    }
}