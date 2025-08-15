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
public class ContactController : ControllerBase
{
    private readonly ILogger<ContactController> _logger;

    public ContactController(ILogger<ContactController> logger)
    {
        _logger = logger;
    }

    [HttpPost(Name = "SubmitContactInquiry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitContactInquiry(
        [FromBody] SubmitContactInquiryModel model,
        [FromServices] IOptions<ContactMailConfiguration> contactMailConfiguration,
        [FromServices] ISendMailService sendMailService,
        [FromServices] IUserConsentService userConsentService,
        [FromServices] IDateTimeService dateTimeService)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }

        if (model.AcceptedDataPolicy.GetValueOrDefault() != true)
        {
            return BadRequest("Data policy needs to be accepted");
        }

        // Bot check
        if (string.IsNullOrEmpty(model.Company) == false)
        {
            _logger.LogInformation("Received contact inquiry likely submitted by a bot.");
            return Ok();
        }

        _logger.LogInformation("Got contact inquiry");

        var userConsentResult = await userConsentService.StoreUserConsentForContactFormAsync(model.Email, dateTimeService.UtcNow()).ConfigureAwait(false);
        if (userConsentResult.IsFailed)
        {
            // do not do anything when storing consent failed
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        await sendMailService
            .SendContactMailAsync(
                contactMailConfiguration.Value.Sender,
                contactMailConfiguration.Value.Receiver,
                new ContactInquiry()
                {
                    Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name,
                    PhoneNumber = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone,
                    Email = model.Email,
                    Text = model.Message,
                })
            .ConfigureAwait(false);

        return Ok();
    }

    public class SubmitContactInquiryModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user accepted the data policy.
        /// </summary>
        [JsonPropertyName("acceptedDataPolicy")]
        public bool? AcceptedDataPolicy { get; init; } = false;

        [MaxLength(200, ErrorMessage = $"Name cannot exceed 200 characters.")]
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        /// <summary>
        /// This value is used to indicate a bot usage. The user cannot set this field so having a value indicates the data was submitted by a non-human.
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Company cannot exceed 1000 characters.")]
        [JsonPropertyName("company")]
        public string? Company { get; init; }

        [MaxLength(200, ErrorMessage = "Phone number cannot exceed 200 characters.")]
        [JsonPropertyName("phone")]
        public string? Phone { get; init; }

        [Required(ErrorMessage = "Email address is required.")]
        [MaxLength(300, ErrorMessage = "Email address cannot exceed 300 characters.")]
        [EmailAddress(ErrorMessage = "Email address is not valid.")]
        [JsonPropertyName("email")]
        public required string Email { get; init; }

        [Required(ErrorMessage = "Message is required.")]
        [MaxLength(2000, ErrorMessage = "Message cannot exceed 2000 characters.")]
        [JsonPropertyName("message")]
        public required string Message { get; init; }
    }
}