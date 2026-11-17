using System.Net;

using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Errors;
using AdoExport.Client.AdoApi.Models;
using AdoExport.Client.Tests.Support;
using AdoExport.Common.Errors;

using FluentResults;

using Microsoft.Extensions.Logging.Abstractions;

using static AdoExport.Client.Tests.Support.AdoApiClientFactory;

namespace AdoExport.Client.Tests.AdoApi;

public class RetrieveAuditDataServiceTests
{
    private static RetrieveAuditDataService CreateService(IAdoApiClient client)
        => new(NullLogger<RetrieveAuditDataService>.Instance, client);

    private static HttpRequestException Http(HttpStatusCode? status) => new("boom", null, status);

    [Test]
    public void Constructor_rejects_null_dependencies()
    {
        FluentActions.Invoking(() => new RetrieveAuditDataService(null!, Mock.Of<IAdoApiClient>())).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new RetrieveAuditDataService(NullLogger<RetrieveAuditDataService>.Instance, null!)).Should().Throw<ArgumentNullException>();
    }

    // ---- error mapping: what the user ends up seeing ---------------------------------------

    [Test]
    public async Task Unauthorized_becomes_InvalidPatError()
    {
        var client = new Mock<IAdoApiClient>();
        client.Setup(c => c.GetAllUsersAsync(It.IsAny<CancellationToken>())).ThrowsAsync(Http(HttpStatusCode.Unauthorized));

        var result = await CreateService(client.Object).GetRawAuditDataAsync(CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.HasError<InvalidPatError>().Should().BeTrue();
        result.HasError<InsufficientRightsError>().Should().BeFalse();
        result.HasError<OrganizationNotFoundError>().Should().BeFalse();
    }

    [Test]
    public async Task Forbidden_becomes_InsufficientRightsError()
    {
        var client = new Mock<IAdoApiClient>();
        client.Setup(c => c.GetAllUsersAsync(It.IsAny<CancellationToken>())).ThrowsAsync(Http(HttpStatusCode.Forbidden));

        var result = await CreateService(client.Object).GetRawAuditDataAsync(CancellationToken.None);

        result.HasError<InsufficientRightsError>().Should().BeTrue();
        result.HasError<InvalidPatError>().Should().BeFalse();
    }

    [Test]
    public async Task NotFound_becomes_OrganizationNotFoundError()
    {
        var client = new Mock<IAdoApiClient>();
        client.Setup(c => c.GetAllUsersAsync(It.IsAny<CancellationToken>())).ThrowsAsync(Http(HttpStatusCode.NotFound));

        var result = await CreateService(client.Object).GetRawAuditDataAsync(CancellationToken.None);

        result.HasError<OrganizationNotFoundError>().Should().BeTrue();
    }

    [TestCase(HttpStatusCode.InternalServerError)]
    [TestCase(null)]
    public async Task Other_http_failures_become_a_generic_failure_carrying_the_exception(HttpStatusCode? status)
    {
        var client = new Mock<IAdoApiClient>();
        var exception = Http(status);
        client.Setup(c => c.GetAllUsersAsync(It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        var result = await CreateService(client.Object).GetRawAuditDataAsync(CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.HasError<InvalidPatError>().Should().BeFalse();
        result.HasError<InsufficientRightsError>().Should().BeFalse();
        result.HasError<OrganizationNotFoundError>().Should().BeFalse();
        result.Errors.Single().Message.Should().Be("Failed gathering data for audit");
        result.Errors.Single().Should().BeOfType<ExceptionalError>().Which.Exception.Should().BeSameAs(exception);
    }

    [Test]
    public async Task Cancellation_requested_by_the_caller_becomes_RequestWasCanceledError()
    {
        using var cts = new CancellationTokenSource();
        var client = new Mock<IAdoApiClient>();
        client.Setup(c => c.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                cts.Cancel();
                throw new OperationCanceledException(cts.Token);
            });

        var result = await CreateService(client.Object).GetRawAuditDataAsync(cts.Token);

        result.RequestWasCanceled().Should().BeTrue();
    }

    [Test]
    public async Task Cancellation_not_requested_by_the_caller_is_not_swallowed()
    {
        var client = new Mock<IAdoApiClient>();
        client.Setup(c => c.GetAllUsersAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new TaskCanceledException("http timeout"));

        var act = () => CreateService(client.Object).GetRawAuditDataAsync(CancellationToken.None);

        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Test]
    public async Task A_401_from_the_real_client_reaches_the_user_as_InvalidPatError()
    {
        // End to end through AdoApiClient: guards the link between EnsureSuccessStatusCode and the error mapping.
        var client = Create(StubHttpMessageHandler.Status(HttpStatusCode.Unauthorized));

        var result = await CreateService(client).GetRawAuditDataAsync(CancellationToken.None);

        result.HasError<InvalidPatError>().Should().BeTrue();
    }

    [Test]
    public async Task A_404_from_the_real_client_reaches_the_user_as_OrganizationNotFoundError()
    {
        var client = Create(StubHttpMessageHandler.Status(HttpStatusCode.NotFound));

        var result = await CreateService(client).GetRawAuditDataAsync(CancellationToken.None);

        result.HasError<OrganizationNotFoundError>().Should().BeTrue();
    }

    // ---- happy path ----------------------------------------------------------------------

    [Test]
    public async Task Gathers_and_partitions_the_audit_data()
    {
        var entitled = new UserDto { Descriptor = "aad.entitled", MetaType = "member" };
        var notEntitled = new UserDto { Descriptor = "aad.none", MetaType = "guest" };
        var agent = new UserDto { Descriptor = "svc.agent", MetaType = "agent" };
        UserDto[] allUsers = [entitled, notEntitled, agent];
        GroupDto[] groups = [new() { Descriptor = "vssgp.g" }];
        var memberships = new Dictionary<string, GroupDto[]> { [entitled.Descriptor] = groups };
        var entitlementSummary = new UserEntitlementsDto { Id = "e-1", User = entitled };
        var entitlementDetail = new GetUserEntitlementsByUserEntitlementIdResponseDto { Id = "e-1", User = entitled };
        var privateToken = new SessionTokenWithUser { User = entitled, SessionToken = new SessionTokenDto { DisplayName = "priv", IsPublic = false } };
        var publicToken = new SessionTokenWithUser { User = notEntitled, SessionToken = new SessionTokenDto { DisplayName = "pub", IsPublic = true } };

        var client = new Mock<IAdoApiClient>();
        client.Setup(c => c.GetAllUsersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(allUsers);
        client.Setup(c => c.GetAllGroupsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(groups);
        client.Setup(c => c.MatchUsersWithGroupMembershipsAsync(allUsers, groups, It.IsAny<CancellationToken>())).ReturnsAsync(memberships);
        client.Setup(c => c.SearchUserEntitlementsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([entitlementSummary]);
        client.Setup(c => c.GetUserEntitlementsByUserIdsAsync(
                It.Is<string[]>(ids => ids.SequenceEqual(new[] { "e-1" })), It.IsAny<CancellationToken>()))
            .ReturnsAsync([entitlementDetail]);
        client.Setup(c => c.GetSessionTokenDetailsForPersonalAccessTokenAsync(
                It.IsAny<UserDto[]>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([privateToken, publicToken]);

        var result = await CreateService(client.Object).GetRawAuditDataAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var data = result.Value;
        data.AllUsers.Should().Equal(allUsers);
        data.UserGroupMemberships.Should().BeSameAs(memberships);
        data.UsersWithProjectEntitlements.Should().Equal(entitlementDetail);
        data.PrivateSessionTokenDetailsWithUser.Select(t => t.SessionToken.DisplayName).Should().Equal("priv");
        data.PublicSessionTokenDetailsWithUser.Select(t => t.SessionToken.DisplayName).Should().Equal("pub");

        // Service accounts (agent) have no PATs worth auditing: only member and guest users are asked.
        client.Verify(c => c.GetSessionTokenDetailsForPersonalAccessTokenAsync(
            It.Is<UserDto[]>(users => users.SequenceEqual(new[] { entitled, notEntitled })),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
