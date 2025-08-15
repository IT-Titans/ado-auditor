using System.Collections.Immutable;

using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Services;
using AdoExport.Auditing;
using AdoExport.Auditing.Rules;

using AutoMapper;

namespace AdoExport.Api.Repositories;

public class RulesRepository : IRulesRepository
{
    private readonly INotesOnRulesService _notesOnRulesService;

    private readonly IMapper _mapper;

    public RulesRepository(
        INotesOnRulesService notesOnRulesService,
        IMapper mapper)
    {
        Guard.IsNotNull(notesOnRulesService);
        Guard.IsNotNull(mapper);

        _notesOnRulesService = notesOnRulesService;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<RuleDto>> GetRuleDtosForAsync(ImmutableArray<RuleResult> ruleResults)
    {
        var rules = new List<RuleDto>();
        foreach (var ruleResult in ruleResults.GroupBy(rule => rule.Reason.Id, rule => rule))
        {
            if (ruleResult.Any() == false)
            {
                continue;
            }

            var hint = _notesOnRulesService.GetByRuleReasonId(ruleResult.Key);
            var representativeRuleResult = ruleResult.First();
            rules.Add(new RuleDto()
            {
                Id = representativeRuleResult.Reason.Id,
                Scope = _mapper.Map<AdoExport.Auditing.Categories, Controllers.Models.Categories>(representativeRuleResult.Reason.Category),
                TotalCounts = ruleResult.Count(),
                Severity = _mapper.Map<AdoExport.Auditing.Severities, Controllers.Models.Severities>(representativeRuleResult.Reason.Severity),
                Name = hint.Name,
                Description = hint.Description,
                Recommendation = hint.Recommendations,
            });
        }

        return Task.FromResult<IReadOnlyList<RuleDto>>(rules.AsReadOnly());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<RuleDto>> GetRuleDtosForAsync(ImmutableArray<RuleResult> ruleResults, ImmutableArray<RuleResult> allRuleResults)
    {
        var rules = new List<RuleDto>();
        foreach (var ruleResult in ruleResults)
        {
            var hint = _notesOnRulesService.GetByRuleReasonId(ruleResult.Reason.Id);
            rules.Add(new RuleDto()
            {
                Id = ruleResult.Reason.Id,
                Scope = _mapper.Map<AdoExport.Auditing.Categories, Controllers.Models.Categories>(ruleResult.Reason.Category),
                TotalCounts = allRuleResults.Count(r => r.Reason.Id == ruleResult.Reason.Id),
                Severity = _mapper.Map<AdoExport.Auditing.Severities, Controllers.Models.Severities>(ruleResult.Reason.Severity),
                Name = hint.Name,
                Description = hint.Description,
                Recommendation = hint.Recommendations,
            });
        }

        return Task.FromResult<IReadOnlyList<RuleDto>>(rules.AsReadOnly());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<RuleDto>> GetRuleDtosForAsync(ImmutableArray<AuditResult> auditResults, ImmutableArray<AuditResult> allAuditResults)
    {
        return GetRuleDtosForAsync(
            [..auditResults.SelectMany(results => results.RuleResults)],
            [ ..allAuditResults.SelectMany(r => r.RuleResults).ToArray()] );
    }
}