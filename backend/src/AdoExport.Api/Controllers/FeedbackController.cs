using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using AdoExport.Api.Configuration;
using AdoExport.Api.Repositories;
using AdoExport.Api.Services;
using AdoExport.Api.Services.Caching;
using AdoExport.Api.Services.Notifications;
using AdoExport.Api.Services.UserConsent;
using AdoExport.Common.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AdoExport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly ILogger<FeedbackController> _logger;

    private readonly IAuditRepository _auditRepository;

    public FeedbackController(
        ILogger<FeedbackController> logger,
        IAuditRepository auditRepository)
    {
        Guard.IsNotNull(logger);
        Guard.IsNotNull(auditRepository);

        _logger = logger;
        _auditRepository = auditRepository;
    }

    [HttpPost(Name = "SubmitFeedback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(typeof(string), StatusCodes.Status410Gone)]
    public async Task<IActionResult> SubmitFeedback(
        [FromQuery] Guid auditId,
        [FromBody] SubmitFeedbackModel model,
        [FromServices] ScopedAuditKeyBearer auditKeyBearer,
        [FromServices] IOptions<FeedbackMailConfiguration> feedbackMailConfiguration,
        [FromServices] ISendMailService sendMailService,
        [FromServices] IUserConsentService userConsentService,
        [FromServices] IDateTimeService dateTimeService)
    {
        if (auditId == Guid.Empty)
        {
            return BadRequest("Audit-Id is required");
        }

        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }

        if (model.AcceptedDataPolicy.GetValueOrDefault() == false)
        {
            return BadRequest("Data policy needs to be accepted");
        }

        return await auditKeyBearer.ScopedAsync<IActionResult>(auditId.ToString(), async () =>
        {
            var auditResult = await _auditRepository.GetAuditByIdAsync(auditKeyBearer.AuditKey).ConfigureAwait(false);
            if (auditResult.IsFailed)
            {
                // currently, we accept when an audit cannot be found to not discourage users from sending feedback when their audit has timed out.
                _logger.LogInformation("Audit for feedback could not be found. Accepting feedback anyway");
                // return this.AuditExpiredStatusCode();
            }

            _logger.LogInformation("Got feedback for audit {AuditId}", auditKeyBearer.AuditKey);

            var userConsentResult = await userConsentService
                .StoreUserConsentForFeedbackFormAsync(
                    auditId.ToString(),
                    model.Email ?? "unknown",
                    dateTimeService.UtcNow())
                .ConfigureAwait(false);
            if (userConsentResult.IsFailed)
            {
                // do not do anything when storing consent failed
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            await sendMailService
                .SendFeedbackMailAsync(
                    feedbackMailConfiguration.Value.Sender,
                    feedbackMailConfiguration.Value.Receiver,
                    new Feedback()
                    {
                        AuditId = auditId.ToString(),
                        Name = model.Name,
                        Email = model.Email,
                        Text = model.Feedback,
                    })
                .ConfigureAwait(false);

            return Ok();
        }).ConfigureAwait(false);
    }

    public class SubmitFeedbackModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user accepted the data policy.
        /// </summary>
        [JsonPropertyName("acceptedDataPolicy")]
        public bool? AcceptedDataPolicy { get; set; } = false;

        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the users optional email address.
        /// </summary>
        [MaxLength(300, ErrorMessage = "Email address cannot exceed 300 characters.")]
        [EmailAddress(ErrorMessage = "Email address is not valid.")]
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Feedback is required.")]
        [MaxLength(3000, ErrorMessage = "Feedback cannot exceed 3000 characters.")]
        [JsonPropertyName("feedback")]
        public required string Feedback { get; set; }
    }
}