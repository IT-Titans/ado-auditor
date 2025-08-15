using System.Runtime.Serialization;

using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Client.AdoApi;

/// <summary>
/// Client to perform various actions on the Microsoft apis.
/// </summary>
public interface IAdoApiClient
{
    /// <summary>
    /// Gets a single <see cref="GetUserEntitlementsByUserEntitlementIdResponseDto" /> for the provided <paramref name="userEntitlementId" />.
    /// </summary>
    /// <remarks>
    /// See: https://learn.microsoft.com/en-us/rest/api/azure/devops/memberentitlementmanagement/user-entitlements/get?view=azure-devops-rest-7.1
    /// </remarks>
    Task<GetUserEntitlementsByUserEntitlementIdResponseDto> GetUserEntitlementsByUserIdAsync(string userEntitlementId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an array of <see cref="GetUserEntitlementsByUserEntitlementIdResponseDto" /> for the provided <paramref name="userEntitlementIds" />.
    /// </summary>
    Task<GetUserEntitlementsByUserEntitlementIdResponseDto[]> GetUserEntitlementsByUserIdsAsync(string[] userEntitlementIds, CancellationToken cancellationToken);

    /// <summary>
    /// Get a list of all users in a given scope.
    /// </summary>
    /// <remarks>
    /// See: https://learn.microsoft.com/en-us/rest/api/azure/devops/graph/users/list?view=azure-devops-rest-7.1
    /// </remarks>
    /// <param name="cancellationToken" />
    Task<UserDto[]> GetAllUsersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a list of all session token details of the (PATs, SSH Keys, or all, depending on <paramref name="isPublic" />) for a particular user identified by <paramref name="subjectDescriptor" />.<br />
    /// Note that as far as we are aware, only users of type <see cref="UserDto.MemberUserMetaType" /> or <see cref="UserDto.GuestUserMetaType" /> can have session token details.<br />
    /// Requests to a member other than that will not fail but yield no results. 
    /// </summary>
    /// <remarks>
    /// See: https://learn.microsoft.com/en-us/rest/api/azure/devops/tokenadmin/personal-access-tokens/list?view=azure-devops-rest-7.1
    /// </remarks>
    /// <param name="subjectDescriptor">The descriptor of the target user.</param>
    /// <param name="isPublic">When not provided (null), gets PAT and SSH tokens. Set to false for PAT tokens only and true for SSH tokens only.</param>
    /// <param name="cancellationToken" />
    Task<SessionTokenDto[]> GetSessionTokenDetailsForPersonalAccessTokenAsync(string subjectDescriptor, bool? isPublic, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a list of all session token details of the (PATs, SSH Keys, or all, depending on <paramref name="isPublic" />) for the given <paramref name="users" />.<br />
    /// Note that as far as we are aware, only users of type <see cref="UserDto.MemberUserMetaType" /> or <see cref="UserDto.GuestUserMetaType" /> can have session token details.<br />
    /// Requests to a member other than that will not fail but yield no results. 
    /// </summary>
    /// <remarks>
    /// See: https://learn.microsoft.com/en-us/rest/api/azure/devops/tokenadmin/personal-access-tokens/list?view=azure-devops-rest-7.1
    /// </remarks>
    /// <param name="users">The list of users to get session token details for.</param>
    /// <param name="isPublic">When not provided (null), gets PAT and SSH tokens. Set to false for PAT tokens only and true for SSH tokens only.</param>
    /// <param name="cancellationToken" />
    Task<SessionTokenWithUser[]> GetSessionTokenDetailsForPersonalAccessTokenAsync(UserDto[] users, bool? isPublic, CancellationToken cancellationToken);

    /// <summary>
    /// Get a paged set of user entitlements matching the filter and sort criteria built with properties that match the select input.
    /// </summary>
    /// <remarks>
    /// See: https://learn.microsoft.com/en-us/rest/api/azure/devops/memberentitlementmanagement/user-entitlements/get?view=azure-devops-rest-7.1
    /// </remarks>
    /// <param name="cancellationToken" />
    Task<UserEntitlementsDto[]> SearchUserEntitlementsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a list of all groups in the current scope (usually organization or account).
    /// </summary>
    /// <remarks>
    /// See: https://learn.microsoft.com/en-us/rest/api/azure/devops/graph/groups/list?view=azure-devops-rest-7.1
    /// </remarks>
    /// <param name="cancellationToken" />
    Task<GroupDto[]> GetAllGroupsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Get a membership relationship between a container and subject.
    /// </summary>
    /// <param name="subjectDescriptor">The subjects descriptor to get memberships for. Can, e.g., be a <see cref="UserDto.Descriptor" />.</param>
    /// <param name="cancellationToken" />
    /// <remarks>
    /// See: https://learn.microsoft.com/en-us/rest/api/azure/devops/graph/memberships/get?view=azure-devops-rest-7.1
    /// </remarks>
    Task<GraphMembershipDto[]> GetMembershipsAsync(string subjectDescriptor, CancellationToken cancellationToken);

    Task<Dictionary<string, GroupDto[]>> MatchUsersWithGroupMembershipsAsync(UserDto[] usersToMatch, GroupDto[] allGroups, CancellationToken cancellationToken);
}