using System.Text.Json.Serialization;

namespace AdoExport.Client.AdoApi.Models;

#nullable disable

public class AccessLevelDto
{
    [JsonPropertyName("licensingSource")]
    public string LicensingSource { get; set; }

    [JsonPropertyName("accountLicenseType")]
    public string AccountLicenseType { get; set; }

    [JsonPropertyName("msdnLicenseType")]
    public string MsdnLicenseType { get; set; }

    [JsonPropertyName("gitHubLicenseType")]
    public string GitHubLicenseType { get; set; }

    [JsonPropertyName("licenseDisplayName")]
    public string LicenseDisplayName { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("statusMessage")]
    public string StatusMessage { get; set; }

    [JsonPropertyName("assignmentSource")]
    public string AssignmentSource { get; set; }
}