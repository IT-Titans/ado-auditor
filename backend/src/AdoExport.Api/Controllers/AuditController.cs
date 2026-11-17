using System.Globalization;

using AdoExport.Api.Configuration;
using AdoExport.Api.Controllers.Extensions;
using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Controllers.Transformers;
using AdoExport.Api.Repositories;
using AdoExport.Api.Services;
using AdoExport.Api.Services.Notifications;
using AdoExport.Api.Services.UserConsent;
using AdoExport.Client.AdoApi.Errors;
using AdoExport.Common.Errors;
using AdoExport.Common.Services;

using AdoExport.Api.RateLimiting;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace AdoExport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly ILogger<AuditController> _logger;

    private readonly IOptions<AuditLogsMailConfiguration> _auditLogsMailOptions;

    private readonly ISendMailService _sendMailService;

    public AuditController(
        ILogger<AuditController> logger,
        IOptions<AuditLogsMailConfiguration> auditLogsMailOptions,
        ISendMailService sendMailService)
    {
        Guard.IsNotNull(logger);
        Guard.IsNotNull(auditLogsMailOptions);
        Guard.IsNotNull(sendMailService);

        _logger = logger;
        _auditLogsMailOptions = auditLogsMailOptions;
        _sendMailService = sendMailService;
    }

    [EnableRateLimiting(RateLimitPolicies.Audit)]
    [HttpPost(Name = "StartAudit")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StartAudit(
        [FromBody] StartAuditModel model,
        CancellationToken cancellationToken,
        [FromServices] IAuditService auditService,
        [FromServices] ScopedAuditKeyBearer auditKeyBearer,
        [FromServices] ScopedUserCredentialsBearer userCredentialsBearer,
        [FromServices] IUserConsentService userConsentService,
        [FromServices] IDateTimeService dateTimeService)
    {
        if (model.AcceptedDataPolicy.GetValueOrDefault() != true)
        {
            return BadRequest("Consent for privacy policy is required");
        }

        if (string.IsNullOrWhiteSpace(model.Organization))
        {
            return BadRequest("Organization is required");
        }

        if (string.IsNullOrWhiteSpace(model.Pat))
        {
            return BadRequest("PAT is required");
        }

        if (string.IsNullOrWhiteSpace(model.Email))
        {
            return BadRequest("Email is required");
        }

        var auditId = Guid.NewGuid();

        var storeUserConsentResult = await userConsentService
            .StoreUserConsentForAuditAsync(
                auditId.ToString(),
                model.Email,
                model.AcceptedAnonymousAuditLogging.GetValueOrDefault(),
                dateTimeService.UtcNow())
            .ConfigureAwait(false);
        if (storeUserConsentResult.IsFailed)
        {
            // do not do anything when storing consent failed
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var auditKey = auditId.ToString();

        return await auditKeyBearer.ScopedAsync<IActionResult>(auditKey, async () =>
        {
            var metrics = new AuditMetrics()
            {
                AuditId = auditId.ToString(),
            };

            if (model.AcceptedAnonymousAuditLogging.GetValueOrDefault())
            {
                // only add the organization name if the user gave explicit consent to anonymous logging
                metrics.Add("organizationName", model.Organization);
            }

            try
            {
                var auditResult = await userCredentialsBearer.ScopedAsync(model.Organization, model.Pat, () => auditService.CreateAuditAsync(auditId, cancellationToken)).ConfigureAwait(false);
                if (auditResult.IsFailed)
                {
                    if (auditResult.RequestWasCanceled())
                    {
                        metrics.Add("failed", "user canceled audit");
                        throw new OperationCanceledException(cancellationToken);
                    }

                    if (auditResult.HasError<OrganizationNotFoundError>())
                    {
                        metrics.Add("failed", "organization not found");
                        return Unauthorized("Organization not found");
                    }

                    if (auditResult.HasError<InsufficientRightsError>())
                    {
                        metrics.Add("failed", "invalid pat scopes / user lacks permissions");
                        return Unauthorized("PAT scopes are invalid or user lacks permissions");
                    }

                    if (auditResult.HasError<InvalidPatError>())
                    {
                        metrics.Add("failed", "invalid pat");
                        return Unauthorized("PAT is invalid");
                    }

                    metrics.Add("failed", auditResult.ToString());
                    _logger.LogError("Error occurred during audit creation: {result}", auditResult);

                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                var audit = auditResult.Value;

                metrics
                    .Add("requestedAt", audit.Metadata.RequestedAuditAt.ToString(CultureInfo.InvariantCulture))
                    .Add("createdAt", audit.Metadata.CreatedAuditAt.ToString(CultureInfo.InvariantCulture))
                    .Add("apisInUseFor", (audit.Metadata.FinishRequestAuditDataAt - audit.Metadata.StartRequestAuditDataAt).ToString());

                return Ok(auditId);
            }
            finally
            {
                // Not awaited: the metrics are for us, and the user must neither wait for
                // them nor see an error because of them. A failure ends up in the log.
                _ = BackgroundMail.Send(
                    () => _sendMailService.SendAuditMetricsMailAsync(
                        _auditLogsMailOptions.Value.Sender,
                        _auditLogsMailOptions.Value.Receiver,
                        metrics),
                    _logger,
                    "audit metrics");
            }
        }).ConfigureAwait(false);
    }

    [HttpGet("{auditId:guid}", Name = "GetAuditSummaries")]
    [ProducesResponseType(typeof(AuditResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status410Gone)]
    public async Task<IActionResult> GetAuditSummary(
        Guid auditId,
        [FromServices] AuditControllerTransformer transformer,
        [FromServices] ScopedAuditKeyBearer auditKeyBearer,
        [FromServices] IAuditRepository auditRepository)
    {
        if (auditId == Guid.Empty)
        {
            return BadRequest("Audit-Id is required");
        }

        var auditKey = auditId.ToString();
        return await auditKeyBearer.ScopedAsync<IActionResult>(auditKey, async () =>
        {
            var auditResult = await auditRepository.GetAuditByIdAsync(auditId).ConfigureAwait(false);
            if (auditResult.IsFailed)
            {
                return this.AuditExpiredStatusCode();
            }

            return Ok(transformer.TransformAuditToAuditResultDto(auditResult.Value));
        }).ConfigureAwait(false);
    }
}