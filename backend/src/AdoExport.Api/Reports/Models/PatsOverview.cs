using AdoExport.Api.Controllers.Models;

namespace AdoExport.Api.Reports.Models;

public class PatsOverview
{
    public required PatDetailsDto[] Pats { get; init; }
}