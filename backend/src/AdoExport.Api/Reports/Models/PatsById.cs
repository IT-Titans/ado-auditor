using AdoExport.Api.Repositories;

namespace AdoExport.Api.Reports.Models;

public class PatsById
{
    public required Dictionary<string, GetPatById> Pats { get; init; }
}