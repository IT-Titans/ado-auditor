using FluentResults;

namespace AdoExport.Api.Repositories;

public interface IPatsFacade
{
    Task<Result<GetAllPats>> GetAllPatsAsync();

    Task<Result<GetPatById>> GetPatByIdAsync(string patId);

    Task<Result<Dictionary<string, GetPatById>>> GetAllPatsByIdAsync();
}