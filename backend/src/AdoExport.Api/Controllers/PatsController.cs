using AdoExport.Api.Auditing;
using AdoExport.Api.Controllers.Extensions;
using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Controllers.Sorting;
using AdoExport.Api.Errors;
using AdoExport.Api.Repositories;
using AdoExport.Api.Services;

using Microsoft.AspNetCore.Mvc;

namespace AdoExport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatsController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    private readonly IAuditRepository _auditRepository;

    private readonly IRulesRepository _rulesRepository;

    private readonly IPatsFacade _patsFacade;

    public PatsController(
        IUserRepository userRepository,
        IAuditRepository auditRepository,
        IRulesRepository rulesRepository,
        IPatsFacade patsFacade)
    {
        Guard.IsNotNull(userRepository);
        Guard.IsNotNull(auditRepository);
        Guard.IsNotNull(rulesRepository);
        Guard.IsNotNull(patsFacade);

        _userRepository = userRepository;
        _auditRepository = auditRepository;
        _rulesRepository = rulesRepository;
        _patsFacade = patsFacade;
    }

    [HttpGet("{patId}", Name = "GetPatDetailsByPatId")]
    [ProducesResponseType(typeof(PatDetailsWithRulesByPatIdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status410Gone)]
    public async Task<IActionResult> GetPatDetailsByPatId(
        string patId,
        [FromQuery] Guid auditId,
        [FromServices] ScopedAuditKeyBearer auditKeyBearer)
    {
        if (string.IsNullOrWhiteSpace(patId))
        {
            return BadRequest("Pat-Id is required");
        }

        if (auditId == Guid.Empty)
        {
            return BadRequest("Audit-Id is required");
        }

        var auditKey = auditId.ToString();
        return await auditKeyBearer.ScopedAsync<IActionResult>(auditKey, async () =>
        {
            var getPatByIdResult = await _patsFacade.GetPatByIdAsync(patId).ConfigureAwait(false);
            if (getPatByIdResult.IsFailed)
            {
                if (getPatByIdResult.HasError<AuditExpiredError>())
                {
                    return this.AuditExpiredStatusCode();
                }
                else if (getPatByIdResult.HasError<PatNotFoundError>())
                {
                    return NotFound("PAT not found");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            var getPatById = getPatByIdResult.Value;

            var audit = getPatById.Audit;
            var pat = getPatById.Pat;
            var rules = getPatById.Rules;
            return Ok(new PatDetailsWithRulesByPatIdDto()
            {
                OrganizationName = audit.OrganizationName,
                RequestedAtDateString = audit.RequestedAtFormatted(),
                CreatedAtDateString = audit.CreatedAtFormatted(),
                ValidUntilDateString = audit.ValidUntilFormatted(),
                Pat = pat,
                Rules = rules
                    .OrderBySeverity()
                    .ToArray(),
            });
        }).ConfigureAwait(false);
    }

    [HttpGet(Name = "GetAllPats")]
    [ProducesResponseType(typeof(AllPatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status410Gone)]
    public async Task<IActionResult> GetAllPats(
        [FromQuery] Guid auditId,
        [FromServices] ScopedAuditKeyBearer auditKeyBearer)
    {
        if (auditId == Guid.Empty)
        {
            return BadRequest("Audit-Id is required");
        }

        var auditKey = auditId.ToString();
        return await auditKeyBearer.ScopedAsync<IActionResult>(auditKey, async () =>
        {
            var getAllPatsResult = await _patsFacade.GetAllPatsAsync().ConfigureAwait(false);
            if (getAllPatsResult.IsFailed)
            {
                if (getAllPatsResult.HasError<AuditExpiredError>())
                {
                    return this.AuditExpiredStatusCode();
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            var getAllPats = getAllPatsResult.Value;

            var audit = getAllPats.Audit;
            var pats = getAllPats.Pats;
            return Ok(new AllPatsDto()
            {
                OrganizationName = audit.OrganizationName,
                RequestedAtDateString = audit.RequestedAtFormatted(),
                CreatedAtDateString = audit.CreatedAtFormatted(),
                ValidUntilDateString = audit.ValidUntilFormatted(),
                Pats = pats
                    .OrderByTotals()
                    .ToArray(),
            });
        }).ConfigureAwait(false);
    }
}