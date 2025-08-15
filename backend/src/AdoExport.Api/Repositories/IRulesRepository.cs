using System.Collections.Immutable;

using AdoExport.Api.Auditing;
using AdoExport.Api.Controllers.Models;
using AdoExport.Auditing;
using AdoExport.Auditing.Rules;

namespace AdoExport.Api.Repositories;

public interface IRulesRepository
{
    Task<IReadOnlyList<RuleDto>> GetRuleDtosForAsync(ImmutableArray<RuleResult> ruleResults, ImmutableArray<RuleResult> allRuleResults);

    Task<IReadOnlyList<RuleDto>> GetRuleDtosForAsync(ImmutableArray<AuditResult> auditResults, ImmutableArray<AuditResult> allAuditResults);

    Task<IReadOnlyList<RuleDto>> GetRuleDtosForAsync(ImmutableArray<RuleResult> ruleResults);
}