using AdoExport.Api.Auditing;
using AdoExport.Api.Controllers.Models;
using AdoExport.Auditing;

namespace AdoExport.Api.Repositories;

public class GetUserEntries
{
    public required Audit Audit { get; init; }

    public required UserEntryDto[] UserEntries { get; init; }
}