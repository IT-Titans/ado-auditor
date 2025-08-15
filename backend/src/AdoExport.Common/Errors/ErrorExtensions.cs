using FluentResults;

namespace AdoExport.Common.Errors;

/// <summary>
/// A set of extension methods related to the errors.
/// </summary>
public static class ErrorExtensions
{
    /// <summary>
    /// Determines whether the result is failed due to the user cancelling the request.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns>True when the result is failed because the user canceled the request; otherwise false.</returns>
    public static bool RequestWasCanceled(this ResultBase result)
    {
        Guard.IsNotNull(result);

        return result.IsSuccess == false && result.HasError<RequestWasCanceledError>();
    }
}