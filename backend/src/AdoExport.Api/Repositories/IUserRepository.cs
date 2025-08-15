using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Api.Repositories;

public interface IUserRepository
{
    /// <summary>
    /// Gets the user that is associated to the given <paramref name="userId" />.<br />
    /// The <paramref name="userId" /> is a specific id that is tied to a <see cref="SessionTokenDto" />.
    /// </summary>
    /// <param name="userId">The user id to get the associated user for.</param>
    /// <returns>The associated user if any; otherwise null.</returns>
    Task<UserDto?> GetUserByUserIdAsync(string userId);

    /// <summary>
    /// Gets all users from the organization.<br />
    /// This includes "real" users such as members and guests but "implicit" users such as build agents etc. as well.
    /// </summary>
    /// <returns>A list of all users.</returns>
    Task<UserDto[]> GetAllUsersAsync();

    /// <summary>
    /// Gets the <see cref="AccessLevelDto" /> for the given <paramref name="userDescriptor" />.<br />
    /// Only users that have entitlements have an <see cref="AccessLevelDto" /> associated. 
    /// </summary>
    /// <param name="userDescriptor">The users descriptor used to identify.</param>
    Task<AccessLevelDto?> GetAccessLevelByUserDescriptorAsync(string userDescriptor);

    /// <summary>
    /// Gets the user that is identified by the given <paramref name="userDescriptor" />. 
    /// </summary>
    /// <param name="userDescriptor">The user descriptor.</param>
    Task<UserDto?> GetUserByDescriptorAsync(string userDescriptor);
}