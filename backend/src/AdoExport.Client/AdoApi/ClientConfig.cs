namespace AdoExport.Client.AdoApi;

public class ClientConfig
{
    /// <summary>
    /// Gets the organization name that is used for the client to authorize.
    /// </summary>
    public required string OrganizationName { get; init; }

    /// <summary>
    /// Gets the PAT that is used for the client to authorize.
    /// </summary>
    public required string Pat { get; init; }
}