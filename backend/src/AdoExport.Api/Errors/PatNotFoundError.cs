using FluentResults;

namespace AdoExport.Api.Errors;

/// <summary>
/// Describes that a PAT could not be found.
/// </summary>
public class PatNotFoundError : Error
{
    public PatNotFoundError(string patId)
    {
        Metadata.Add(nameof(patId), patId);
    }
}