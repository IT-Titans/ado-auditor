using FluentResults;

namespace AdoExport.Common.Errors;

/// <summary>
/// Describes that a request was canceled.
/// </summary>
public class RequestWasCanceledError : Error
{
    public RequestWasCanceledError()
    {
    }

    public RequestWasCanceledError(IError causedBy)
        : base("User canceled request", causedBy)
    {
    }
}