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
public class UsersController : ControllerBase
{
    private readonly IUsersFacade _usersFacade;

    public UsersController(
        IUsersFacade usersFacade)
    {
        Guard.IsNotNull(usersFacade);

        _usersFacade = usersFacade;
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="auditId">The audit id.</param>
    /// <param name="auditKeyBearer" />
    [HttpGet(Name = "GetAllUsers")]
    [ProducesResponseType(typeof(AllUsersDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status410Gone)]
    public async Task<IActionResult> GetAllUsers(
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
            var getUsersResult = await _usersFacade.GetUserEntriesAsync().ConfigureAwait(false);
            if (getUsersResult.IsFailed)
            {
                return this.AuditExpiredStatusCode();
            }

            var audit = getUsersResult.Value.Audit;
            var userEntries = getUsersResult.Value.UserEntries;

            return Ok(new AllUsersDto()
            {
                OrganizationName = audit.OrganizationName,
                RequestedAtDateString = audit.RequestedAtFormatted(),
                CreatedAtDateString = audit.CreatedAtFormatted(),
                ValidUntilDateString = audit.ValidUntilFormatted(),
                Users = userEntries
                    .OrderByTotals()
                    .ToArray(),
            });
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets user details for a specific user identified by <paramref name="userId" />.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="auditId">The audit id.</param>
    /// <param name="auditKeyBearer" />
    [HttpGet("{userId}", Name = "GetUserDetailsByUserId")]
    [ProducesResponseType(typeof(UserDetailsByUserIdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status410Gone)]
    public async Task<IActionResult> GetUserDetailsByUserId(
        string userId,
        [FromQuery] Guid auditId,
        [FromServices] ScopedAuditKeyBearer auditKeyBearer)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("User-Id is required");
        }

        if (auditId == Guid.Empty)
        {
            return BadRequest("Audit-Id is required");
        }

        var auditKey = auditId.ToString();
        return await auditKeyBearer.ScopedAsync<IActionResult>(auditKey, async () =>
        {
            var getUserResult = await _usersFacade.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (getUserResult.IsFailed)
            {
                if (getUserResult.HasError<AuditExpiredError>())
                {
                    return this.AuditExpiredStatusCode();
                }
                else if (getUserResult.HasError<UserNotFoundError>())
                {
                    return NotFound("User not found");
                }
                else if (getUserResult.HasError<AccessLevelNotFoundError>())
                {
                    return NotFound("Access level not found");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }

            var audit = getUserResult.Value.Audit;
            var user = getUserResult.Value.User;
            var patsSummary = getUserResult.Value.PatsSummary;
            var pats = getUserResult.Value.Pats;
            var rules = getUserResult.Value.Rules;

            return Ok(new UserDetailsByUserIdDto()
            {
                OrganizationName = audit.OrganizationName,
                RequestedAtDateString = audit.RequestedAtFormatted(),
                CreatedAtDateString = audit.CreatedAtFormatted(),
                ValidUntilDateString = audit.ValidUntilFormatted(),
                User = user,
                PatsSummary = patsSummary,
                Pats = pats
                    .OrderByTotals()
                    .ToArray(),
                Rules = rules
                    .OrderBySeverity()
                    .ToArray(),
            });
        }).ConfigureAwait(false);
    }
}