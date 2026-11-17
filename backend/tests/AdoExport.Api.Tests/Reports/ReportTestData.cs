using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Reports.Models;
using AdoExport.Api.Repositories;
using Audit = AdoExport.Auditing.Audit;
using AuditingMetadata = AdoExport.Auditing.AuditMetadata;

namespace AdoExport.Api.Tests.Reports;

/// <summary>
/// Builds report input that looks like what a real audit produces, so that the whole
/// component tree gets exercised instead of a hand-picked corner of it.
/// </summary>
internal static class ReportTestData
{
    public static readonly DateTime AuditCreatedAt = new(2026, 3, 14, 9, 30, 0, DateTimeKind.Utc);

    public static Audit CreateAudit(string organizationName = "Contoso")
        => new(new AuditingMetadata
        {
            AuditId = Guid.NewGuid(),
            OrganizationName = organizationName,
            ExpiresAfter = TimeSpan.FromHours(1),
            CreatedAuditAt = AuditCreatedAt,
        })
        {
            PatAuditResults = [],
            SshKeysAuditResults = [],
            UserAuditResults = [],
        };

    public static TotalSeverityCountsDto Counts(int criticals, int warnings, int oks)
        => new() { TotalCriticals = criticals, TotalWarnings = warnings, TotalOks = oks };

    public static RuleDto Rule(
        string name,
        Severities severity,
        Categories scope = Categories.Users,
        int totalCounts = 1,
        string? description = null,
        string? recommendation = null)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Severity = severity,
            Scope = scope,
            TotalCounts = totalCounts,
            Description = description ?? $"Description of {name}",
            Recommendation = recommendation ?? $"Recommendation for {name}",
        };

    public static PatDetailsDto Pat(
        string id,
        string name,
        string userId = "user-1",
        string userName = "Alice Admin",
        string[]? scopes = null,
        TotalSeverityCountsDto? counts = null)
        => new()
        {
            Id = id,
            Name = name,
            User = new PatDetailsUserDto { UserId = userId, Name = userName },
            Scopes = scopes ?? ["vso.code", "vso.build"],
            CreatedAt = "2026-01-05T08:00:00Z",
            ExpiresIn = "2026-07-05T08:00:00Z",
            AffectedPatRulesCounts = counts ?? Counts(1, 1, 1),
        };

    public static UserDetailsDto UserDetails(string id, string userName, bool isActive = true, string metaType = "member")
        => new() { Id = id, UserName = userName, IsActive = isActive, MetaType = metaType };

    public static UserEntryDto UserEntry(string id, string userName, bool isActive = true, TotalSeverityCountsDto? counts = null)
        => new()
        {
            Id = id,
            UserName = userName,
            IsActive = isActive,
            MetaType = "member",
            AffectedUserRulesCounts = counts ?? Counts(1, 0, 2),
        };

    public static GetUserById UserById(UserDetailsDto user, PatDetailsDto[] pats, RuleDto[] rules)
        => new()
        {
            Audit = CreateAudit(),
            User = user,
            PatsSummary = Counts(0, 0, 0),
            Pats = pats,
            Rules = rules,
        };

    public static GetPatById PatById(PatDetailsDto pat, RuleDto[] rules)
        => new() { Audit = CreateAudit(), Pat = pat, Rules = rules };

    /// <summary>
    /// A model with every section filled: all three severities, several rule categories,
    /// an active and an inactive user, a user without any PAT and a PAT without rules.
    /// </summary>
    public static AuditPdfModel CreateFullModel(string organizationName = "Contoso")
    {
        var patRules = new[]
        {
            Rule("PAT has full access", Severities.Critical, Categories.Pats),
            Rule("PAT never rotated", Severities.Warning, Categories.Pats),
            Rule("PAT scope is narrow", Severities.Ok, Categories.Pats),
        };
        var userRules = new[]
        {
            Rule("User has no MFA", Severities.Critical, Categories.Users),
            Rule("User is inactive", Severities.Warning, Categories.Users),
        };
        var projectRule = Rule("Project is public", Severities.Warning, Categories.Projects);

        var alicePat = Pat("pat-1", "Build pipeline token", counts: Counts(1, 1, 1));
        var bobPat = Pat("pat-2", "Quiet token", "user-2", "Bob Builder", counts: Counts(0, 0, 0));

        var alice = UserDetails("user-1", "Alice Admin");
        var bob = UserDetails("user-2", "Bob Builder", isActive: false, metaType: "serviceAccount");

        return new AuditPdfModel
        {
            Audit = new Api.Reports.Models.AuditMetadata { CreatedAt = AuditCreatedAt },
            Organization = new Organization { Name = organizationName },
            RulesOverview = new RulesOverview { Rules = [.. patRules, .. userRules, projectRule] },
            UsersOverview = new UsersOverview
            {
                Users =
                [
                    UserEntry("user-1", "Alice Admin", counts: Counts(1, 1, 0)),
                    UserEntry("user-2", "Bob Builder", isActive: false, counts: Counts(0, 0, 0)),
                ],
            },
            UsersById = new UsersById
            {
                Users = new Dictionary<string, GetUserById>
                {
                    ["user-1"] = UserById(alice, [alicePat], userRules),
                    ["user-2"] = UserById(bob, [], []),
                },
            },
            PatsOverview = new PatsOverview { Pats = [alicePat, bobPat] },
            PatsById = new PatsById
            {
                Pats = new Dictionary<string, GetPatById>
                {
                    ["pat-1"] = PatById(alicePat, patRules),
                    ["pat-2"] = PatById(bobPat, []),
                },
            },
        };
    }

    public static AuditPdfModel CreateEmptyModel(string organizationName = "Contoso")
        => new()
        {
            Audit = new Api.Reports.Models.AuditMetadata { CreatedAt = AuditCreatedAt },
            Organization = new Organization { Name = organizationName },
            RulesOverview = new RulesOverview { Rules = [] },
            UsersOverview = new UsersOverview { Users = [] },
            UsersById = new UsersById { Users = [] },
            PatsOverview = new PatsOverview { Pats = [] },
            PatsById = new PatsById { Pats = [] },
        };
}
