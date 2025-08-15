using FluentResults;

namespace AdoExport.Api.Repositories;

public interface IUsersFacade
{
    Task<Result<GetUserEntries>> GetUserEntriesAsync();

    Task<Result<GetUserById>> GetUserByIdAsync(string userId);

    Task<Result<Dictionary<string, GetUserById>>> GetAllUsersByIdAsync();
}