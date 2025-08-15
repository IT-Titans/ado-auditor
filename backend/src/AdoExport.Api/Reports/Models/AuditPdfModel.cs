namespace AdoExport.Api.Reports.Models;

public class AuditPdfModel
{
    public required AuditMetadata Audit { get; init; }

    public required Organization Organization { get; init; }

    public required RulesOverview RulesOverview { get; init; }

    public required PatsOverview PatsOverview { get; init; }

    public required PatsById PatsById { get; init; }

    public required UsersOverview UsersOverview { get; init; }

    public required UsersById UsersById { get; init; }
}