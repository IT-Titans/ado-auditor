using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Api.Repositories;

public interface ISessionTokenRepository
{
    /// <summary>
    /// Gets all session tokens for a specific user identified by <paramref name="userDescriptor" />.
    /// </summary>
    /// <param name="userDescriptor">The users descriptor to match the session tokens for.</param>
    /// <returns>An unordered array of all session tokens for the given user.</returns>
    Task<SessionTokenDto[]> GetAllSessionTokensByUserDescriptorAsync(string userDescriptor);

    /// <summary>
    /// Gets all PATs (private session tokens) for a specific user identified by <paramref name="userDescriptor" />.
    /// </summary>
    /// <param name="userDescriptor">The users descriptor to match the session tokens for.</param>
    /// <returns>An unordered array of all PATs for the given user.</returns>
    Task<SessionTokenDto[]> GetPatsByUserDescriptorAsync(string userDescriptor);

    /// <summary>
    /// Gets all ssh keys (public session tokens) for a specific user identified by <paramref name="userDescriptor" />.
    /// </summary>
    /// <param name="userDescriptor">The users descriptor to match the session tokens for.</param>
    /// <returns>An unordered array of all ssh keys for the given user.</returns>
    Task<SessionTokenDto[]> GetSshKeysByUserDescriptorAsync(string userDescriptor);
}