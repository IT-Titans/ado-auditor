using System.Net;

using AdoExport.Client.AdoApi.Errors;
using AdoExport.Client.AdoApi.Models;
using AdoExport.Common.Errors;

using FluentResults;

using Microsoft.Extensions.Logging;

namespace AdoExport.Client.AdoApi;

public sealed class RetrieveAuditDataService : IRetrieveAuditDataService
{
    private readonly ILogger<RetrieveAuditDataService> _logger;

    private readonly IAdoApiClient _client;

    public RetrieveAuditDataService(
        ILogger<RetrieveAuditDataService> logger,
        IAdoApiClient apiClient)
    {
        Guard.IsNotNull(logger);
        Guard.IsNotNull(apiClient);

        _logger = logger;
        _client = apiClient;
    }

    /// <inheritdoc />
    public async Task<Result<RawAuditData>> GetRawAuditDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await GatherDataAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            switch (e.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return Result.Fail(new OrganizationNotFoundError());
                case HttpStatusCode.Forbidden:
                    return Result.Fail(new InsufficientRightsError("PAT scopes are invalid or user lacks permissions"));
                case HttpStatusCode.Unauthorized:
                    return Result.Fail(new InvalidPatError());
                default:
                    _logger.LogError(e, "Failed to gather data to audit organization");
                    return Result.Fail(new ExceptionalError("Failed gathering data for audit", e));
            }
        }
        catch (OperationCanceledException e) when (cancellationToken.IsCancellationRequested)
        {
            return Result.Fail(new RequestWasCanceledError(new ExceptionalError(e)));
        }
    }

    private async Task<RawAuditData> GatherDataAsync(CancellationToken cancellationToken)
    {
        var allUsers = await _client.GetAllUsersAsync(cancellationToken).ConfigureAwait(false);

        var allGroups = await _client.GetAllGroupsAsync(cancellationToken).ConfigureAwait(false);

        var userGroupMemberships = await _client.MatchUsersWithGroupMembershipsAsync(allUsers, allGroups, cancellationToken).ConfigureAwait(false);

        var allUserEntitlements = await _client.SearchUserEntitlementsAsync(cancellationToken).ConfigureAwait(false);

        var allSingleUserEntitlementsResponse = await _client
            .GetUserEntitlementsByUserIdsAsync(
                allUserEntitlements
                    .Select(userEntitlement => userEntitlement.Id).ToArray(),
                cancellationToken);

        var usersWithoutEntitlements = allUsers
            .Where(user =>
                allUserEntitlements.Any(entitlement => entitlement.User.Descriptor == user.Descriptor) == false)
            .ToArray();

        var usersWithEntitlements = allUsers.Except(usersWithoutEntitlements).ToArray();

        var usersWithProjectEntitlement = usersWithEntitlements
            .Join(
                allSingleUserEntitlementsResponse,
                user => user.Descriptor,
                entitlement => entitlement.User.Descriptor,
                (user, entitlement) => new UserWithProjectEntitlementMapping()
                { User = user, Entitlement = entitlement, })
            .ToArray();

        var allSessionTokens = await _client.GetSessionTokenDetailsForPersonalAccessTokenAsync(
            allUsers.Where(user => user.IsRealUserType()).ToArray(),
            isPublic: null,
            cancellationToken);

        var allPrivateSessionTokenDetails = allSessionTokens.Where(token => token.SessionToken.IsPublic == false).ToArray();
        var allPublicSessionTokenDetails = allSessionTokens.Where(token => token.SessionToken.IsPublic == true).ToArray();

        return new RawAuditData()
        {
            AllUsers = allUsers,
            UserGroupMemberships = userGroupMemberships,
            UsersWithProjectEntitlements = usersWithProjectEntitlement.Select(mapping => mapping.Entitlement).ToArray(),
            PrivateSessionTokenDetailsWithUser = allPrivateSessionTokenDetails.Select(tokenDetails => new SessionTokenWithUser()
            {
                SessionToken = tokenDetails.SessionToken,
                User = tokenDetails.User,
            }).ToArray(),
            PublicSessionTokenDetailsWithUser = allPublicSessionTokenDetails.Select(token => new SessionTokenWithUser()
            {
                SessionToken = token.SessionToken,
                User = token.User,
            }).ToArray(),
        };
    }
}