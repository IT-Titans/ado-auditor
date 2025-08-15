using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

using AdoExport.Client.AdoApi.Helpers;
using AdoExport.Client.AdoApi.Models;

using Microsoft.Extensions.Logging;

// ReSharper disable ConvertClosureToMethodGroup

namespace AdoExport.Client.AdoApi;

public sealed class AdoApiClient : IAdoApiClient
{
    private const string ApiVersion = "7.1-preview.1";

    private readonly ILogger _logger;

    private readonly IHttpClientFactory _clientFactory;

    private readonly IClientConfigProvider _clientConfigProvider;

    private readonly Lock _getSessionTokenDetailsForPersonalAccessTokenLock = new Lock();

    public AdoApiClient(
        ILogger<AdoApiClient> logger,
        IHttpClientFactory clientFactory,
        IClientConfigProvider clientConfigProvider)
    {
        Guard.IsNotNull(logger);
        Guard.IsNotNull(clientFactory);
        Guard.IsNotNull(clientConfigProvider);

        _logger = logger;
        _clientFactory = clientFactory;
        _clientConfigProvider = clientConfigProvider;
    }

    /// <inheritdoc />
    public async Task<GetUserEntitlementsByUserEntitlementIdResponseDto> GetUserEntitlementsByUserIdAsync(string userEntitlementId, CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(userEntitlementId);

        var clientConfig = await GetClientConfigAsync().ConfigureAwait(false);

        _logger.LogTrace("Requesting user entitlements for user id {UserId}", userEntitlementId);

        var responseContent = await RequestAsync(
            QueryHelpers.BuildUrlWithQueryString(
                $"https://vsaex.dev.azure.com/{clientConfig.OrganizationName}/_apis/userentitlements/{userEntitlementId}",
                new Dictionary<string, string?>
                {
                    { "api-version", ApiVersion},
                }))
            .ConfigureAwait(false);

        return JsonSerializer.Deserialize<GetUserEntitlementsByUserEntitlementIdResponseDto>(responseContent) ??
               throw new SerializationException($"Failed to deserialize user entitlements for user id '{userEntitlementId}'");
    }

    /// <inheritdoc />
    public async Task<GetUserEntitlementsByUserEntitlementIdResponseDto[]> GetUserEntitlementsByUserIdsAsync(string[] userEntitlementIds, CancellationToken cancellationToken)
    {
        var entitlements = new ConcurrentQueue<GetUserEntitlementsByUserEntitlementIdResponseDto>();
        await Parallel.ForEachAsync(userEntitlementIds, cancellationToken, async (userEntitlementId, token) =>
            {
                // Get User entitlements for a specific user using the userId -> contains a list of projectEntitlements and groupAssignments
                var userEntitlements = await GetUserEntitlementsByUserIdAsync(userEntitlementId, token)
                    .ConfigureAwait(false);

                entitlements.Enqueue(userEntitlements);
            })
            .ConfigureAwait(false);

        return entitlements.ToArray();
    }

    /// <inheritdoc />
    public async Task<UserDto[]> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        var clientConfig = await GetClientConfigAsync().ConfigureAwait(false);

        _logger.LogTrace("Requesting all users");

        var users = new List<UserDto>();

        string? continuationToken = null;
        do
        {
            _logger.LogTrace("Requesting all users {WithOrWithout} continuationToken", (string.IsNullOrWhiteSpace(continuationToken) ? "without" : "with"));

            var responseContent = await RequestAsync(
                     QueryHelpers.BuildUrlWithQueryString(
                         $"https://vssps.dev.azure.com/{clientConfig.OrganizationName}/_apis/graph/users",
                         new Dictionary<string, string?>
                         {
                             { "continuationToken", continuationToken },
                             { "api-version", ApiVersion},
                         })
                     )
                .ConfigureAwait(false);

            var allUsersResponse = JsonSerializer.Deserialize<GetAllUsersResponseDto>(responseContent) ??
                                   throw new SerializationException("Failed to deserialize all users");

            users.AddRange(allUsersResponse.Value);
            continuationToken = allUsersResponse.ContinuationToken;
        } while (string.IsNullOrEmpty(continuationToken) == false);

        return users.ToArray();
    }

    /// <inheritdoc />
    public async Task<SessionTokenDto[]> GetSessionTokenDetailsForPersonalAccessTokenAsync(string subjectDescriptor, bool? isPublic, CancellationToken cancellationToken)
    {
        var clientConfig = await GetClientConfigAsync().ConfigureAwait(false);

        _logger.LogTrace("Requesting Session Token Details for Personal Access Token for subject descriptor {SubjectDescriptor}", subjectDescriptor);
        var sessionTokens = new List<SessionTokenDto>();

        string? continuationToken = null;
        do
        {
            _logger.LogTrace("Requesting all {PublicOrPrivate} session token details {WithOrWithout} continuationToken", (isPublic.HasValue == false ? "all" : isPublic.Value ? "public" : "private"), (string.IsNullOrWhiteSpace(continuationToken) ? "without" : "with"));

            var queryParams = new Dictionary<string, string?>
            {
                { "continuationToken", continuationToken },
                { "api-version", ApiVersion },
            };

            if (isPublic.HasValue)
            {
                queryParams.Add("isPublic", isPublic.ToString());
            }

            var responseContent = await RequestAsync(
                    QueryHelpers.BuildUrlWithQueryString(
                        $"https://vssps.dev.azure.com/{clientConfig.OrganizationName}/_apis/tokenadmin/personalaccesstokens/{subjectDescriptor}",
                        queryParams))
                .ConfigureAwait(false);

            var allSessionTokensResponse = JsonSerializer.Deserialize<GetSessionTokenDetailsForPersonalAccessTokenResponseDto>(responseContent) ??
                                           throw new SerializationException("Failed to deserialize all session token details");

            sessionTokens.AddRange(allSessionTokensResponse.Value);

            continuationToken = allSessionTokensResponse.ContinuationToken;
        } while (string.IsNullOrEmpty(continuationToken) == false);
        return sessionTokens.OfType<SessionTokenDto>().ToArray();
    }

    /// <inheritdoc />
    public async Task<SessionTokenWithUser[]> GetSessionTokenDetailsForPersonalAccessTokenAsync(UserDto[] users, bool? isPublic, CancellationToken cancellationToken)
    {
        var sessionTokenDetails = new ConcurrentQueue<SessionTokenWithUser>();
        await Parallel.ForEachAsync(users, cancellationToken, async (user, token) =>
            {
                // Get User entitlements for a specific user using the userId -> contains a list of projectEntitlements and groupAssignments
                var sessionTokenDetailsResponse = await GetSessionTokenDetailsForPersonalAccessTokenAsync(user.Descriptor, isPublic, token)
                    .ConfigureAwait(false);

                // todo: handle continuation

                lock (_getSessionTokenDetailsForPersonalAccessTokenLock)
                {
                    foreach (var sessionTokenDetail in sessionTokenDetailsResponse)
                    {
                        sessionTokenDetails.Enqueue(new SessionTokenWithUser()
                        {
                            User = user,
                            SessionToken = sessionTokenDetail,
                        });
                    }
                }
            })
            .ConfigureAwait(false);

        return sessionTokenDetails.ToArray();
    }

    /// <inheritdoc />
    public async Task<UserEntitlementsDto[]> SearchUserEntitlementsAsync(CancellationToken cancellationToken)
    {
        var clientConfig = await GetClientConfigAsync().ConfigureAwait(false);

        _logger.LogTrace("Requesting all user entitlements");

        var responseContent = await RequestAsync(
            QueryHelpers.BuildUrlWithQueryString(
                $"https://vsaex.dev.azure.com/{clientConfig.OrganizationName}/_apis/userentitlements",
                new Dictionary<string, string?>
                {
                    { "api-version", ApiVersion},
                }))
            .ConfigureAwait(false);

        var userEntitlementsResponse = JsonSerializer.Deserialize<GetAllUserEntitlementsResponseDto>(responseContent) ??
               throw new SerializationException("Failed to deserialize all user entitlements");

        return userEntitlementsResponse.Value;
    }

    /// <inheritdoc />
    public async Task<GroupDto[]> GetAllGroupsAsync(CancellationToken cancellationToken)
    {
        var clientConfig = await GetClientConfigAsync().ConfigureAwait(false);

        var groups = new List<GroupDto>();

        string? continuationToken = null;
        do
        {
            _logger.LogTrace("Requesting all groups {WithOrWithout} continuationToken", (string.IsNullOrWhiteSpace(continuationToken) ? "without" : "with"));

            var responseContent = await RequestAsync(
                    QueryHelpers.BuildUrlWithQueryString(
                        $"https://vssps.dev.azure.com/{clientConfig.OrganizationName}/_apis/graph/groups",
                        new Dictionary<string, string?>
                        {
                            { "continuationToken", continuationToken },
                            { "api-version", ApiVersion},
                        })
                )
                .ConfigureAwait(false);

            var allGroupsResponse = JsonSerializer.Deserialize<GetAllGroupsResponseDto>(responseContent) ??
                                           throw new SerializationException("Failed to deserialize all groups");

            groups.AddRange(allGroupsResponse.Value);

            continuationToken = allGroupsResponse.ContinuationToken;
        } while (string.IsNullOrEmpty(continuationToken) == false);

        return groups.ToArray();
    }

    /// <inheritdoc />
    public async Task<GraphMembershipDto[]> GetMembershipsAsync(string subjectDescriptor, CancellationToken cancellationToken)
    {
        var clientConfig = await GetClientConfigAsync().ConfigureAwait(false);

        _logger.LogTrace("Requesting memberships for subjectDescriptor {Descriptor}", subjectDescriptor);

        var responseContent = await RequestAsync(
            QueryHelpers.BuildUrlWithQueryString(
                $"https://vssps.dev.azure.com/{clientConfig.OrganizationName}/_apis/graph/memberships/{subjectDescriptor}",
                new Dictionary<string, string?>
                {
                    { "api-version", ApiVersion},
                }))
            .ConfigureAwait(false);

        var membershipsResponse = JsonSerializer.Deserialize<GetSubjectMembershipsResponseDto>(responseContent) ??
               throw new SerializationException("Failed to deserialize subject memberships groups");

        return membershipsResponse.Value;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, GroupDto[]>> MatchUsersWithGroupMembershipsAsync(UserDto[] usersToMatch, GroupDto[] allGroups, CancellationToken cancellationToken)
    {
        var userGroupMemberships = new ConcurrentDictionary<string, GroupDto[]>();
        await Parallel.ForEachAsync(usersToMatch, cancellationToken, async (user, token) =>
            {
                var memberships = await GetMembershipsAsync(user.Descriptor, token).ConfigureAwait(false);
                var groups = new List<GroupDto>();
                foreach (var membership in memberships)
                {
                    groups.AddRange(allGroups.Where(group => group.Descriptor == membership.ContainerDescriptor));
                }

                userGroupMemberships.TryAdd(user.Descriptor, groups.ToArray());
            })
            .ConfigureAwait(false);

        return userGroupMemberships.ToDictionary();
    }

    private Task<ClientConfig> GetClientConfigAsync()
    {
        return _clientConfigProvider.GetConfigAsync();
    }

    private async Task<HttpClient> GetAuthorizedClientAsync()
    {
        var clientConfig = await GetClientConfigAsync().ConfigureAwait(false);

        var client = _clientFactory.CreateClient();

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "AdoAuditor.App");
        client.DefaultRequestHeaders.Add("X-TFS-FedAuthRedirect", "Suppress");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"PAT:{clientConfig.Pat}")));

        return client;
    }

    private async Task<string> RequestAsync(string requestUri)
    {
        var client = await GetAuthorizedClientAsync().ConfigureAwait(false);

        using var response = await client.GetAsync(requestUri).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
}