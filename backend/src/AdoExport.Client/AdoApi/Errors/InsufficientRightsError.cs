using FluentResults;

namespace AdoExport.Client.AdoApi.Errors;

public class InsufficientRightsError : Error
{
    public InsufficientRightsError()
    {
    }

    public InsufficientRightsError(string message)
        : base(message)
    {
    }

    public InsufficientRightsError(string message, IError causedBy)
        : base(message, causedBy)
    {
    }
}