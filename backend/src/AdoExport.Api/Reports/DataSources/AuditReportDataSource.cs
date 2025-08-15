using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Reports.Models;
using AdoExport.Api.Repositories;
using AdoExport.Api.Services;
using AdoExport.Auditing;

using FluentResults;

namespace AdoExport.Api.Reports.DataSources;

public class AuditReportDataSource : IAuditReportDataSource
{
    private readonly IAuditRepository _auditRepository;

    private readonly IRulesRepository _rulesRepository;

    private readonly IUsersFacade _usersFacade;

    private readonly IPatsFacade _patsFacade;

    private readonly ScopedAuditKeyBearer _scopedAuditKeyBearer;

    public AuditReportDataSource(
        IAuditRepository auditRepository,
        IRulesRepository rulesRepository,
        IUsersFacade usersFacade,
        IPatsFacade patsFacade,
        ScopedAuditKeyBearer scopedAuditKeyBearer)
    {
        Guard.IsNotNull(auditRepository);
        Guard.IsNotNull(rulesRepository);
        Guard.IsNotNull(usersFacade);
        Guard.IsNotNull(patsFacade);
        Guard.IsNotNull(scopedAuditKeyBearer);

        _auditRepository = auditRepository;
        _rulesRepository = rulesRepository;
        _usersFacade = usersFacade;
        _patsFacade = patsFacade;
        _scopedAuditKeyBearer = scopedAuditKeyBearer;
    }

    /// <inheritdoc />
    public async Task<Result<AuditPdfModel>> GetAuditDataAsync(CancellationToken cancellationToken)
    {
        var auditResult = await _auditRepository.GetAuditByIdAsync(_scopedAuditKeyBearer.AuditKey).ConfigureAwait(false);
        if (auditResult.IsFailed)
        {
            return auditResult.ToResult();
        }
        var audit = auditResult.Value;

        var getUsersResult = await _usersFacade.GetUserEntriesAsync().ConfigureAwait(false);
        if (getUsersResult.IsFailed)
        {
            return getUsersResult.ToResult();
        }
        var getUserEntries = getUsersResult.Value;

        var getUsersByIdResult = await _usersFacade.GetAllUsersByIdAsync().ConfigureAwait(false);
        if (getUsersByIdResult.IsFailed)
        {
            return getUsersByIdResult.ToResult();
        }
        var getUsersById = getUsersByIdResult.Value;

        var getAllPatsResult = await _patsFacade.GetAllPatsAsync().ConfigureAwait(false);
        if (getAllPatsResult.IsFailed)
        {
            return getAllPatsResult.ToResult();
        }
        var getAllPats = getAllPatsResult.Value;

        var getPatsByIdResult = await _patsFacade.GetAllPatsByIdAsync().ConfigureAwait(false);
        if (getPatsByIdResult.IsFailed)
        {
            return getPatsByIdResult.ToResult();
        }
        var getPatsById = getPatsByIdResult.Value;

        var rules = await GetAllRulesAsync(audit).ConfigureAwait(false);
        return new AuditPdfModel()
        {
            Audit = new Models.AuditMetadata()
            {
                CreatedAt = audit.Metadata.CreatedAuditAt,
            },
            Organization = new Organization()
            {
                Name = audit.OrganizationName,
            },
            RulesOverview = new RulesOverview()
            {
                Rules = rules,
            },
            UsersOverview = new UsersOverview()
            {
                Users = getUserEntries.UserEntries,
            },
            UsersById = new UsersById()
            {
                Users = getUsersById,
            },
            PatsOverview = new PatsOverview()
            {
                Pats = getAllPats.Pats,
            },
            PatsById = new PatsById()
            {
                Pats = getPatsById,
            },
        };
    }

    private async Task<RuleDto[]> GetAllRulesAsync(Audit audit)
    {
        IEnumerable<AuditResult> allAuditRuleResults = (audit.PatAuditResults.Concat(audit.SshKeysAuditResults)).OfType<AuditResult>().Concat(audit.UserAuditResults);
        var ruleResults = allAuditRuleResults.SelectMany(rule => rule.RuleResults);

        var rules = await _rulesRepository
            .GetRuleDtosForAsync([..ruleResults])
            .ConfigureAwait(false);

        return rules.ToArray();
    }
}