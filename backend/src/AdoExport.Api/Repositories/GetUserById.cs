using AdoExport.Api.Auditing;
using AdoExport.Api.Controllers.Models;
using AdoExport.Auditing;

namespace AdoExport.Api.Repositories;

public class GetUserById
{
    public required Audit Audit { get; init; }

    public required UserDetailsDto User { get; init; }

    public required TotalSeverityCountsDto PatsSummary { get; init; }

    public required PatDetailsDto[] Pats { get; init; }

    public required RuleDto[] Rules { get; init; }
}