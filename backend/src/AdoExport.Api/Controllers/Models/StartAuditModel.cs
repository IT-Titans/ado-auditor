using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AdoExport.Api.Controllers.Models;

public class StartAuditModel
{
    /// <summary>
    /// Gets or sets the organization name.<br />
    /// This identifies the organization to audit.
    /// </summary>
    [Required(ErrorMessage = "Organization name is required.")]
    [MaxLength(300, ErrorMessage = "Organization name cannot exceed 300 characters.")]
    [JsonPropertyName("organization")]
    public required string Organization { get; init; }

    /// <summary>
    /// Gets or sets the PAT used to authenticate at the Azure DevOps APIs.
    /// </summary>
    [Required(ErrorMessage = "PAT is required.")]
    [MaxLength(100, ErrorMessage = "PAT cannot exceed 100 characters.")]
    [JsonPropertyName("pat")]
    public required string Pat { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the user actively gave consent to processing data according to the privacy policy.
    /// </summary>
    [JsonPropertyName("acceptedDataPolicy")]
    public required bool? AcceptedDataPolicy { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the user actively gave consent to anonymous audit logging.
    /// </summary>
    [JsonPropertyName("acceptedAnonymousAuditLogging")]
    public required bool? AcceptedAnonymousAuditLogging { get; init; }

    /// <summary>
    /// Gets or sets the users email address.
    /// </summary>
    [Required(ErrorMessage = "Email address is required.")]
    [MaxLength(300, ErrorMessage = "Email address cannot exceed 300 characters.")]
    [EmailAddress(ErrorMessage = "Email address is not valid.")]
    [JsonPropertyName("email")]
    public required string Email { get; init; }
}