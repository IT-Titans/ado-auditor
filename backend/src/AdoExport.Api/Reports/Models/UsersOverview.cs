using AdoExport.Api.Controllers.Models;

namespace AdoExport.Api.Reports.Models;

public class UsersOverview
{
    public required UserEntryDto[] Users { get; init; }
}