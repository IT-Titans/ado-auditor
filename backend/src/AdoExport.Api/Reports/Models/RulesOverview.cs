using AdoExport.Api.Controllers.Models;

namespace AdoExport.Api.Reports.Models;

public class RulesOverview
{
    public required RuleDto[] Rules { get; init; }
}