using AdoExport.Api.Auditing;
using AdoExport.Api.Controllers.Extensions;
using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Controllers.Sorting;
using AdoExport.Api.Formatters;
using AdoExport.Api.Repositories;
using AdoExport.Api.Services;
using AdoExport.Auditing;
using AdoExport.Client.AdoApi.Models;

using AdoExport.Api.Controllers.Mappings;

using Microsoft.AspNetCore.Mvc;

namespace AdoExport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RulesController : ControllerBase
{
    private readonly IAuditRepository _auditRepository;

    private readonly IUserRepository _userRepository;

    private readonly IRulesRepository _rulesRepository;

    public RulesController(
        IAuditRepository auditRepository,
        IUserRepository userRepository,
        IRulesRepository rulesRepository)
    {
        Guard.IsNotNull(auditRepository);
        Guard.IsNotNull(userRepository);
        Guard.IsNotNull(rulesRepository);

        _auditRepository = auditRepository;
        _userRepository = userRepository;
        _rulesRepository = rulesRepository;
    }

    /// <summary>
    /// Gets all PAT rules.
    /// </summary>
    /// <param name="auditId">The audit id.</param>
    /// <param name="severity">[Optional] The <see cref="Models.Severities" /> to filter the rules to be returned. When not provided, all PAT rules are returned.</param>
    /// <param name="auditKeyBearer" />
    /// <param name="mapper" />
    [HttpGet("pats", Name = "GetRulesForPatsWithSeverity")]
    [ProducesResponseType(typeof(PatRulesBySeverityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status410Gone)]
    public async Task<IActionResult> GetPatRulesBySeverity(
        [FromQuery] Guid auditId,
        [FromQuery] Models.Severities? severity,
        [FromServices] ScopedAuditKeyBearer auditKeyBearer,
        [FromServices] IAuditMapper mapper)
    {
        if (auditId == Guid.Empty)
        {
            return BadRequest("Audit-Id is required");
        }

        var auditKey = auditId.ToString();
        return await auditKeyBearer.ScopedAsync<IActionResult>(auditKey, async () =>
        {
            var auditResult = await _auditRepository.GetAuditByIdAsync(auditId).ConfigureAwait(false);
            if (auditResult.IsFailed)
            {
                return this.AuditExpiredStatusCode();
            }

            var audit = auditResult.Value;

            var ruleResults = (audit.PatAuditResults
                    .Concat(audit.SshKeysAuditResults))
                .SelectMany(rule => rule.RuleResults)
                .Where(result =>
                {
                    if (severity.HasValue == false)
                    {
                        return true;
                    }

                    return result.Reason.Severity == mapper.MapSeverityToDomain(severity.Value);
                });

            var rules = await _rulesRepository
                .GetRuleDtosForAsync([..ruleResults])
                .ConfigureAwait(false);

            return Ok(new PatRulesBySeverityDto()
            {
                OrganizationName = audit.OrganizationName,
                RequestedAtDateString = audit.RequestedAtFormatted(),
                CreatedAtDateString = audit.CreatedAtFormatted(),
                ValidUntilDateString = audit.ValidUntilFormatted(),
                Rules = rules
                    .OrderBySeverity()
                    .ToArray(),
            });
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all PATs to whom a rule with the <paramref name="ruleId"/> could be matched.
    /// </summary>
    /// <param name="ruleId">The rule id to match PATs for.</param>
    /// <param name="auditId">The audit id.</param>
    /// <param name="auditKeyBearer" />
    /// <param name="notesOnRulesService" />
    /// <param name="mapper" />
    [HttpGet("pats/{ruleId:guid}", Name = "GetPatsByRuleId")]
    [ProducesResponseType(typeof(RuleWithPatsByRuleIdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status410Gone)]
    public async Task<IActionResult> GetPatsByRule(
        Guid ruleId,
        [FromQuery] Guid auditId,
        [FromServices] ScopedAuditKeyBearer auditKeyBearer,
        [FromServices] INotesOnRulesService notesOnRulesService,
        [FromServices] IAuditMapper mapper)
    {
        if (ruleId == Guid.Empty)
        {
            return BadRequest("Rule-Id is required");
        }

        if (auditId == Guid.Empty)
        {
            return BadRequest("Audit-Id is required");
        }

        var auditKey = auditId.ToString();
        return await auditKeyBearer.ScopedAsync<IActionResult>(auditKey, async () =>
        {
            var auditResult = await _auditRepository.GetAuditByIdAsync(auditId).ConfigureAwait(false);
            if (auditResult.IsFailed)
            {
                return this.AuditExpiredStatusCode();
            }

            var audit = auditResult.Value;

            // todo: contains both pat and ssh key results. Needs to be split once ssh keys have their own workflow
            var auditResults = audit.PatAuditResults
                .Concat(audit.SshKeysAuditResults).ToArray();

            var ruleResults = auditResults
                .SelectMany(rule => rule.RuleResults)
                .Where(result => result.Reason.Id == ruleId)
                .ToArray();

            if (ruleResults.Any() == false)
            {
                return NotFound("Rule not found");
            }

            var ruleResult = ruleResults.First();
            var auditees = auditResults
                .Where(patResult => patResult.RuleResults.Any(result => result.Reason.Id == ruleResult.Reason.Id))
                .Select(patResult => patResult.Auditee)
                .ToArray();

            var items = new List<PatDetailsDto>();
            foreach (var auditee in auditees)
            {
                var totalAuditeeResults = auditResults
                    .Where(result => result.Auditee == auditee)
                    .ToArray();

                var user = await _userRepository
                    .GetUserByUserIdAsync(auditee.UserId)
                    .ConfigureAwait(false);
                PatDetailsUserDto? patUser = user is null
                    ? null
                    : new PatDetailsUserDto()
                    {
                        UserId = user.Descriptor,
                        Name = user.DisplayName,
                    };

                items.Add(new PatDetailsDto()
                {
                    Id = auditee.AuthorizationId,
                    User = patUser,
                    Name = auditee.DisplayName,
                    CreatedAt = DateTime.Parse(auditee.ValidFrom).FormatForFrontend(),
                    ExpiresIn = DateTime.Parse(auditee.ValidTo).FormatForFrontend(),
                    Scopes = auditee.Scope.Split(' '),
                    AffectedPatRulesCounts = new TotalSeverityCountsDto()
                    {
                        TotalCriticals = totalAuditeeResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Critical),
                        TotalWarnings = totalAuditeeResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Warning),
                        TotalOks = totalAuditeeResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Ok),
                    },
                });
            }

            var hint = notesOnRulesService.GetByRuleReasonId(ruleResult.Reason.Id);
            var rule = new PatRuleWithItemsDto()
            {
                Id = ruleResult.Reason.Id,
                Scope = Models.Categories.Pats,
                TotalCounts = auditees.Count(),
                Severity = mapper.MapSeverityToDto(ruleResult.Reason.Severity),
                Name = hint.Name,
                Description = hint.Description,
                Recommendation = hint.Recommendations,
                Items = items
                    .OrderByTotals()
                    .ToArray(),
            };

            return Ok(new RuleWithPatsByRuleIdDto()
            {
                OrganizationName = audit.OrganizationName,
                RequestedAtDateString = audit.RequestedAtFormatted(),
                CreatedAtDateString = audit.CreatedAtFormatted(),
                ValidUntilDateString = audit.ValidUntilFormatted(),
                Rule = rule,
            });
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all user rules.
    /// </summary>
    /// <param name="auditId">The audit id.</param>
    /// <param name="severity">[Optional] The <see cref="Models.Severities" /> to filter the rules to be returned. When not provided, all user rules are returned.</param>
    /// <param name="auditKeyBearer" />
    /// <param name="mapper" />
    [HttpGet("users", Name = "GetRulesForUsersWithSeverity")]
    [ProducesResponseType(typeof(UserRulesBySeverityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status410Gone)]
    public async Task<IActionResult> GetUsersRulesBySeverity(
        [FromQuery] Guid auditId,
        [FromQuery] Models.Severities? severity,
        [FromServices] ScopedAuditKeyBearer auditKeyBearer,
        [FromServices] IAuditMapper mapper)
    {
        if (auditId == Guid.Empty)
        {
            return BadRequest("Audit-Id is required");
        }

        var auditKey = auditId.ToString();
        return await auditKeyBearer.ScopedAsync<IActionResult>(auditKey, async () =>
        {
            var auditResult = await _auditRepository.GetAuditByIdAsync(auditId).ConfigureAwait(false);
            if (auditResult.IsFailed)
            {
                return this.AuditExpiredStatusCode();
            }

            var audit = auditResult.Value;

            var ruleResults = audit.UserAuditResults
                .SelectMany(rule => rule.RuleResults)
                .Where(result =>
                {
                    if (severity.HasValue == false)
                    {
                        return true;
                    }

                    return result.Reason.Severity == mapper.MapSeverityToDomain(severity.Value);
                });

            var rules = await _rulesRepository
                .GetRuleDtosForAsync([..ruleResults])
                .ConfigureAwait(false);

            return Ok(new UserRulesBySeverityDto()
            {
                OrganizationName = audit.OrganizationName,
                RequestedAtDateString = audit.RequestedAtFormatted(),
                CreatedAtDateString = audit.CreatedAtFormatted(),
                ValidUntilDateString = audit.ValidUntilFormatted(),
                Rules = rules
                    .OrderBySeverity()
                    .ToArray(),
            });
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all users to whom a rule with the <paramref name="ruleId"/> could be matched.
    /// </summary>
    /// <param name="ruleId">The rule id to match users for.</param>
    /// <param name="auditId">The audit id.</param>
    /// <param name="auditKeyBearer" />
    /// <param name="notesOnRulesService" />
    /// <param name="mapper" />
    [HttpGet("users/{ruleId:guid}", Name = "GetUsersByRuleId")]
    [ProducesResponseType(typeof(RuleWithUsersByRuleIdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status410Gone)]
    public async Task<IActionResult> GetUsersByRule(
        Guid ruleId,
        [FromQuery] Guid auditId,
        [FromServices] ScopedAuditKeyBearer auditKeyBearer,
        [FromServices] INotesOnRulesService notesOnRulesService,
        [FromServices] IAuditMapper mapper)
    {
        if (ruleId == Guid.Empty)
        {
            return BadRequest("Rule-Id is required");
        }

        if (auditId == Guid.Empty)
        {
            return BadRequest("Audit-Id is required");
        }

        var auditKey = auditId.ToString();
        return await auditKeyBearer.ScopedAsync<IActionResult>(auditKey, async () =>
        {
            var auditResult = await _auditRepository.GetAuditByIdAsync(auditId).ConfigureAwait(false);
            if (auditResult.IsFailed)
            {
                return this.AuditExpiredStatusCode();
            }

            var audit = auditResult.Value;

            var auditResults = audit.UserAuditResults;

            var ruleResults = auditResults
                .SelectMany(rule => rule.RuleResults)
                .Where(result => result.Reason.Id == ruleId)
                .ToArray();

            if (ruleResults.Any() == false)
            {
                return NotFound("Rule not found");
            }

            var ruleResult = ruleResults.First();
            var auditees = auditResults
                .Where(userResult => userResult.RuleResults.Any(result => result.Reason.Id == ruleResult.Reason.Id))
                .Select(userResult => userResult.Auditee)
                .ToArray();

            var items = new List<UserRuleDetailsDto>();
            foreach (var auditee in auditees)
            {
                var totalAuditeeResults = auditResults
                    .Where(result => result.Auditee == auditee)
                    .ToArray();

                items.Add(new UserRuleDetailsDto()
                {
                    Id = auditee.Descriptor,
                    UserName = auditee.DisplayName,
                    MetaType = auditee.DisplayableMetaType(),
                    AffectedUserRulesCounts = new TotalSeverityCountsDto()
                    {
                        TotalCriticals = totalAuditeeResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Critical),
                        TotalWarnings = totalAuditeeResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Warning),
                        TotalOks = totalAuditeeResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Ok),
                    },
                });
            }

            var hint = notesOnRulesService.GetByRuleReasonId(ruleResult.Reason.Id);
            var rule = new UserRuleWithItemsDto()
            {
                Id = ruleResult.Reason.Id,
                Scope = Models.Categories.Users,
                TotalCounts = auditees.Count(),
                Severity = mapper.MapSeverityToDto(ruleResult.Reason.Severity),
                Name = hint.Name,
                Description = hint.Description,
                Recommendation = hint.Recommendations,
                Items = items
                    .OrderByTotals()
                    .ToArray(),
            };

            return Ok(new RuleWithUsersByRuleIdDto()
            {
                OrganizationName = audit.OrganizationName,
                RequestedAtDateString = audit.RequestedAtFormatted(),
                CreatedAtDateString = audit.CreatedAtFormatted(),
                ValidUntilDateString = audit.ValidUntilFormatted(),
                Rule = rule,
            });
        }).ConfigureAwait(false);
    }
}