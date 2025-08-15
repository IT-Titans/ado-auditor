using FluentResults;

namespace AdoExport.Client.AdoApi.Errors;

public class OrganizationNotFoundError : Error
{
    public OrganizationNotFoundError()
    {
    }

    public OrganizationNotFoundError(string message)
        : base(message)
    {
    }

    public OrganizationNotFoundError(string message, IError causedBy)
        : base(message, causedBy)
    {
    }
}