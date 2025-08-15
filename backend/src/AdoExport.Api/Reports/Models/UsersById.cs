using AdoExport.Api.Repositories;

namespace AdoExport.Api.Reports.Models;

public class UsersById
{
    public required Dictionary<string, GetUserById> Users { get; init; }
}