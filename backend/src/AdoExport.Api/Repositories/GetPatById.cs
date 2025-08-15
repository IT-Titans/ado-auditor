using AdoExport.Api.Auditing;
using AdoExport.Api.Controllers.Models;
using AdoExport.Auditing;

namespace AdoExport.Api.Repositories;

public class GetPatById
{
    public required Audit Audit { get; init; }

    public required PatDetailsDto Pat { get; init; }

    public required RuleDto[] Rules { get; init; }
}