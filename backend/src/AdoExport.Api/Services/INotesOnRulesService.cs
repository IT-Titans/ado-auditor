using AdoExport.Api.Auditing;
using AdoExport.Api.Controllers;
using AdoExport.Auditing;

namespace AdoExport.Api.Services;

public interface INotesOnRulesService
{
    /// <summary>
    /// Gets a rule hint providing information about a rule by its <paramref name="ruleReasonId" />.
    /// </summary>
    /// <param name="ruleReasonId">The rule reason id that identifies the hint.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">When the <paramref name="ruleReasonId" /> was empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">When there is no hint for <paramref name="ruleReasonId" />.</exception>
    RuleHint GetByRuleReasonId(Guid ruleReasonId);
}