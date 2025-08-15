namespace AdoExport.Api.Configuration;

public class AzureCommunicationServiceConfiguration
{
    public const string ConfigurationSectionName = "AzureCommunicationService";

    /// <summary>
    /// The azure communication service connectionstring containing the endpoint and access key.
    /// </summary>
    public required string ConnectionString { get; init; }
}