using AdoExport.Api.Controllers.Models;

namespace AdoExport.Api.Reports.Templates.Components;

public static class Sections
{
    public static string RulesOverviewSectionId() => "rules-overview";

    public static string RuleSectionId(string ruleId) => $"rule-{ruleId}";

    public static string RuleCategoryId(Categories category) => $"rule-category-{category}";

    public static string UsersOverviewSectionId() => "users-overview";

    public static string UsersByIdSectionId() => "user-details";

    public static string UserOverviewSectionId(string userId) => $"user-{userId}";

    public static string PatsOverviewSectionId() => "pats-overview";

    public static string PatsByIdSectionId() => "pats-by-id";

    public static string PatByIdSectionId(string patId) => $"pat-{patId}";
}