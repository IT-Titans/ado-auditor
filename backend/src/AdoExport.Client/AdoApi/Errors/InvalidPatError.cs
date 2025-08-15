using FluentResults;

namespace AdoExport.Client.AdoApi.Errors;

public class InvalidPatError : Error
{
    public InvalidPatError()
    {
    }

    public InvalidPatError(string message)
        : base(message)
    {
    }

    public InvalidPatError(string message, IError causedBy)
        : base(message, causedBy)
    {
    }
}