using AdoExport.Api.Auditing;
using AdoExport.Auditing;
using AdoExport.Auditing.Rules;
using AdoExport.Auditing.Rules.Pats.Reasons;
using AdoExport.Auditing.Rules.SshKeys.Reasons;
using AdoExport.Auditing.Rules.Users.Reasons;

namespace AdoExport.Api.Services;

public class AuditSummarizerService : IAuditSummarizerService
{
    private readonly INotesOnRulesService _notesOnRulesService;

    private const int MaximumNumberOfOrganizationAdmins = 1;

    public AuditSummarizerService(INotesOnRulesService notesOnRulesService)
    {
        _notesOnRulesService = notesOnRulesService;
    }

    /// <inheritdoc />
    public IEnumerable<SummaryItem> Summarize(Audit audit)
    {
        var patResults = audit.PatAuditResults;
        var sshKeysResults = audit.SshKeysAuditResults;
        var userResults = audit.UserAuditResults;

        var summaries = GetSummariesForRuleReason<PatExpiresInOverOneQuarterReason>(patResults, Categories.Pats)
            .Concat(GetSummariesForRuleReason<PatExpiresInOverHalfAYearReason>(patResults, Categories.Pats))
            .Concat(GetSummariesForRuleReason<PatExpiresWithinShortTimeRangeReason>(patResults, Categories.Pats))
            .Concat(GetSummariesForRuleReason<PatIsValidForAllOrganizationsReason>(patResults, Categories.Pats))
            .Concat(GetSummariesForRuleReason<PatIsValidForMoreThanOneOrganizationReason>(patResults, Categories.Pats))
            .Concat(GetSummariesForRuleReason<PatIsValidForOnlyOneOrganizationReason>(patResults, Categories.Pats))
            .Concat(GetSummariesForRuleReason<PatHasAppScopeReason>(patResults, Categories.Pats))
            .Concat(GetSummariesForRuleReason<PatHasLimitedScopeReason>(patResults, Categories.Pats))

            .Concat(GetSummariesForRuleReason<SshKeyExpiresInOverAYearReason>(sshKeysResults, Categories.Pats))
            .Concat(GetSummariesForRuleReason<SshKeyExpiresInOverHalfAYearReason>(sshKeysResults, Categories.Pats))
            .Concat(GetSummariesForRuleReason<SshKeyExpiresWithinShortTimeRangeReason>(sshKeysResults, Categories.Pats))
            .Concat(GetSummariesForRuleReason<SshKeyIsValidForAllOrganizationsReason>(sshKeysResults, Categories.Pats))
            .Concat(GetSummariesForRuleReason<SshKeyIsValidForMoreThanOneOrganizationReason>(sshKeysResults, Categories.Pats))
            .Concat(GetSummariesForRuleReason<SshKeyIsValidForOnlyOneOrganizationReason>(sshKeysResults, Categories.Pats))

            .Concat(GetSummariesForRuleReason<UserIsOfTypeMemberReason>(userResults, Categories.Users))
            .Concat(GetSummariesForRuleReason<UserIsOfTypeGuestReason>(userResults, Categories.Users))
            .Concat(GetSummariesForRuleReason<UserIsOfTypeBuildAgentReason>(userResults, Categories.Users))
            .Concat(GetSummariesForRuleReason<UserIsOfTypeAgentPoolReason>(userResults, Categories.Users))
            .Concat(GetSummariesForRuleReason<UserIsOfTypeUnknownReason>(userResults, Categories.Users))

            .Concat(GetSummariesForRuleReason<UserExceedsOverallPatLimitReason>(userResults, Categories.Users))
            .Concat(GetSummariesForRuleReason<UserIsWithinOverallPatLimitReason>(userResults, Categories.Users))
            .Concat(GetSummariesForRuleReason<UserIsNotOrganizationAdminReason>(userResults, Categories.Users))
            ;

        var organizationAdmins = GetSummariesForRuleReason<UserIsOrganizationAdminReason>(userResults, Categories.Users).ToArray();
        if (organizationAdmins.Sum(summary => summary.AffectedCount) > MaximumNumberOfOrganizationAdmins)
        {
            summaries = summaries.Concat(organizationAdmins);
        }

        return summaries;
    }

    private IEnumerable<SummaryItem> GetSummariesForRuleReason<TReason>(AuditResult[] patResults, Categories category)
        where TReason : AuditRuleReason
    {
        var matchingReasons = patResults
            .SelectMany(r => r.RuleResults)
            .Select(r => r.Reason)
            .OfType<TReason>()
            .ToArray();

        if (matchingReasons.Length == 0)
        {
            yield break;
        }

        var exemplaryReason = matchingReasons.First();
        var ruleHint = _notesOnRulesService.GetByRuleReasonId(exemplaryReason.Id);

        var summaryItem = new SummaryItem()
        {
            Category = category,
            Severity = exemplaryReason.Severity,
            Text = ruleHint.Name,
            AffectedCount = matchingReasons.Count(),
            TotalCount = patResults.Length,
        };

        if (summaryItem.AffectedCount > 0)
        {
            yield return summaryItem;
        }
    }
}