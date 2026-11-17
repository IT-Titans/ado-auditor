using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;
using AdoExport.Client.Tests.Support;

using Microsoft.Extensions.Logging.Abstractions;

using static AdoExport.Client.Tests.Support.AdoApiClientFactory;

namespace AdoExport.Client.Tests.AdoApi;

public class AdoApiClientTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [Test]
    public void Constructor_rejects_null_dependencies()
    {
        var factory = Mock.Of<IHttpClientFactory>();
        var config = Mock.Of<IClientConfigProvider>();

        FluentActions.Invoking(() => new AdoApiClient(null!, factory, config)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new AdoApiClient(NullLogger<AdoApiClient>.Instance, null!, config)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new AdoApiClient(NullLogger<AdoApiClient>.Instance, factory, null!)).Should().Throw<ArgumentNullException>();
    }

    // ---- request construction -------------------------------------------------------------

    [Test]
    public async Task Requests_carry_basic_auth_built_from_the_configured_pat()
    {
        var handler = StubHttpMessageHandler.Json("""{"count":0,"value":[]}""");
        var sut = Create(handler, pat: "abc123");

        await sut.GetAllUsersAsync(Ct);

        var request = handler.Requests.Single();
        request.AuthScheme.Should().Be("Basic");
        Encoding.ASCII.GetString(Convert.FromBase64String(request.AuthParameter!)).Should().Be("PAT:abc123");
    }

    [Test]
    public async Task Requests_send_json_accept_user_agent_and_suppress_the_login_redirect()
    {
        var handler = StubHttpMessageHandler.Json("""{"count":0,"value":[]}""");

        await Create(handler).GetAllUsersAsync(Ct);

        var request = handler.Requests.Single();
        request.Method.Should().Be(HttpMethod.Get);
        request.Accept.Should().Equal("application/json");
        request.UserAgent.Should().Be("AdoAuditor.App");
        // Without this header Azure DevOps answers an invalid PAT with a 203 sign-in page instead of a 401.
        request.FedAuthRedirect.Should().Be("Suppress");
    }

    [Test]
    public async Task GetAllUsers_targets_the_graph_endpoint_of_the_organization()
    {
        var handler = StubHttpMessageHandler.Json("""{"count":0,"value":[]}""");

        await Create(handler, organization: "fabrikam").GetAllUsersAsync(Ct);

        handler.Requests.Single().Url.Should().StartWith("https://vssps.dev.azure.com/fabrikam/_apis/graph/users?")
            .And.Contain("api-version=7.1-preview.1");
    }

    [Test]
    public async Task GetUserEntitlementsByUserId_targets_the_vsaex_endpoint_with_the_id()
    {
        var handler = StubHttpMessageHandler.Json("""{"id":"e-1"}""");

        await Create(handler).GetUserEntitlementsByUserIdAsync("e-1", Ct);

        handler.Requests.Single().Url.Should()
            .Be("https://vsaex.dev.azure.com/contoso/_apis/userentitlements/e-1?api-version=7.1-preview.1");
    }

    [Test]
    public async Task SearchUserEntitlements_targets_the_vsaex_collection_endpoint()
    {
        var handler = StubHttpMessageHandler.Json("""{"count":0,"value":[]}""");

        await Create(handler).SearchUserEntitlementsAsync(Ct);

        handler.Requests.Single().Url.Should()
            .Be("https://vsaex.dev.azure.com/contoso/_apis/userentitlements?api-version=7.1-preview.1");
    }

    [Test]
    public async Task GetMemberships_targets_the_membership_endpoint_of_the_descriptor()
    {
        var handler = StubHttpMessageHandler.Json("""{"count":0,"value":[]}""");

        await Create(handler).GetMembershipsAsync("aad.xyz", Ct);

        handler.Requests.Single().Url.Should()
            .Be("https://vssps.dev.azure.com/contoso/_apis/graph/memberships/aad.xyz?api-version=7.1-preview.1");
    }

    [TestCase(null, ExpectedResult = false)]
    [TestCase(true, ExpectedResult = true)]
    [TestCase(false, ExpectedResult = true)]
    public async Task<bool> GetSessionTokenDetails_sends_isPublic_only_when_given(bool? isPublic)
    {
        var handler = StubHttpMessageHandler.Json("""{"value":[]}""");

        await Create(handler).GetSessionTokenDetailsForPersonalAccessTokenAsync("aad.xyz", isPublic, Ct);

        var url = handler.Requests.Single().Url;
        url.Should().StartWith("https://vssps.dev.azure.com/contoso/_apis/tokenadmin/personalaccesstokens/aad.xyz?");
        if (isPublic.HasValue)
        {
            url.Should().Contain($"isPublic={isPublic.Value}");
        }

        return url.Contains("isPublic=");
    }

    // ---- deserialisation and paging -------------------------------------------------------

    [Test]
    public async Task GetUserEntitlementsByUserId_deserialises_the_full_entitlement()
    {
        const string json = """
            {
              "id": "e-1",
              "user": { "descriptor": "aad.AAAA", "displayName": "Ada", "metaType": "member" },
              "accessLevel": { "licenseDisplayName": "Basic", "status": "active" },
              "projectEntitlements": [
                { "projectRef": { "id": "p1", "name": "Alpha" }, "group": { "groupType": "projectContributor", "displayName": "Contributors" }, "assignmentSource": "unknown" }
              ]
            }
            """;

        var result = await Create(StubHttpMessageHandler.Json(json)).GetUserEntitlementsByUserIdAsync("e-1", Ct);

        result.Id.Should().Be("e-1");
        result.User.DisplayName.Should().Be("Ada");
        result.AccessLevel.IsActive().Should().BeTrue();
        var project = result.ProjectEntitlements.Should().ContainSingle().Subject;
        project.ProjectReference.Name.Should().Be("Alpha");
        project.Group.GroupType.Should().Be("projectContributor");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    public async Task GetUserEntitlementsByUserId_rejects_blank_id_without_calling_the_api(string? id)
    {
        var handler = StubHttpMessageHandler.Json("{}");

        var act = () => Create(handler).GetUserEntitlementsByUserIdAsync(id!, Ct);

        await act.Should().ThrowAsync<ArgumentException>();
        handler.Requests.Should().BeEmpty();
    }

    [Test]
    public async Task GetUserEntitlementsByUserIds_fetches_each_id()
    {
        var handler = new StubHttpMessageHandler(r =>
        {
            var id = r.Url[(r.Url.LastIndexOf('/') + 1)..r.Url.IndexOf('?')];
            return StubHttpMessageHandler.JsonResponse($$"""{"id":"{{id}}"}""");
        });

        var result = await Create(handler).GetUserEntitlementsByUserIdsAsync(["a", "b", "c"], Ct);

        result.Select(e => e.Id).Should().BeEquivalentTo(["a", "b", "c"]);
        handler.Requests.Should().HaveCount(3);
    }

    [Test]
    public async Task GetAllUsers_follows_the_continuation_token_until_it_is_exhausted()
    {
        var handler = new StubHttpMessageHandler(r => r.Url.Contains("continuationToken=page2")
            ? StubHttpMessageHandler.JsonResponse($$"""{"count":1,"value":[{{UserJson("aad.two")}}]}""")
            : StubHttpMessageHandler.JsonResponse($$"""{"count":1,"continuationToken":"page2","value":[{{UserJson("aad.one")}}]}"""));

        var users = await Create(handler).GetAllUsersAsync(Ct);

        users.Select(u => u.Descriptor).Should().Equal("aad.one", "aad.two");
        handler.Requests.Should().HaveCount(2);
        handler.Requests.First().Url.Should().NotContain("continuationToken=page");
        handler.Requests.Last().Url.Should().Contain("continuationToken=page2");
    }

    [Test]
    public async Task GetAllGroups_follows_the_continuation_token_until_it_is_exhausted()
    {
        var handler = new StubHttpMessageHandler(r => r.Url.Contains("continuationToken=g2")
            ? StubHttpMessageHandler.JsonResponse($$"""{"count":1,"value":[{{GroupJson("vssgp.two")}}]}""")
            : StubHttpMessageHandler.JsonResponse($$"""{"count":1,"continuationToken":"g2","value":[{{GroupJson("vssgp.one")}}]}"""));

        var groups = await Create(handler).GetAllGroupsAsync(Ct);

        groups.Select(g => g.Descriptor).Should().Equal("vssgp.one", "vssgp.two");
        handler.Requests.Should().HaveCount(2);
    }

    [Test]
    public async Task GetSessionTokenDetails_follows_the_continuation_token_and_maps_token_fields()
    {
        var handler = new StubHttpMessageHandler(r => r.Url.Contains("continuationToken=t2")
            ? StubHttpMessageHandler.JsonResponse("""{"value":[{"displayName":"second","isValid":false,"isPublic":true}]}""")
            : StubHttpMessageHandler.JsonResponse("""{"continuationToken":"t2","value":[{"displayName":"first","isValid":true,"isPublic":false,"validTo":"2027-01-01T00:00:00Z","scope":"app_token","targetAccounts":["acc-1"]}]}"""));

        var tokens = await Create(handler).GetSessionTokenDetailsForPersonalAccessTokenAsync("aad.x", null, Ct);

        tokens.Should().HaveCount(2);
        tokens[0].DisplayName.Should().Be("first");
        tokens[0].IsValid.Should().BeTrue();
        tokens[0].IsPublic.Should().BeFalse();
        tokens[0].Scope.Should().Be("app_token");
        tokens[0].ValidTo.Should().Be("2027-01-01T00:00:00Z");
        tokens[0].TargetAccounts.Should().Equal("acc-1");
        tokens[1].IsPublic.Should().BeTrue();
        tokens[1].IsValid.Should().BeFalse();
    }

    [Test]
    public async Task GetSessionTokenDetails_for_users_attaches_the_owning_user_to_each_token()
    {
        var ada = new UserDto { Descriptor = "aad.ada" };
        var bob = new UserDto { Descriptor = "aad.bob" };
        var handler = new StubHttpMessageHandler(r => r.Url.Contains("/aad.ada?")
            ? StubHttpMessageHandler.JsonResponse("""{"value":[{"displayName":"ada-1"},{"displayName":"ada-2"}]}""")
            : StubHttpMessageHandler.JsonResponse("""{"value":[{"displayName":"bob-1"}]}"""));

        var result = await Create(handler).GetSessionTokenDetailsForPersonalAccessTokenAsync([ada, bob], null, Ct);

        result.Should().HaveCount(3);
        result.Where(t => t.User == ada).Select(t => t.SessionToken.DisplayName).Should().BeEquivalentTo(["ada-1", "ada-2"]);
        result.Where(t => t.User == bob).Select(t => t.SessionToken.DisplayName).Should().BeEquivalentTo(["bob-1"]);
    }

    [Test]
    public async Task SearchUserEntitlements_returns_the_value_array()
    {
        const string json = """{"count":1,"value":[{"id":"e-9","user":{"descriptor":"aad.nine"}}]}""";

        var result = await Create(StubHttpMessageHandler.Json(json)).SearchUserEntitlementsAsync(Ct);

        result.Should().ContainSingle().Which.Id.Should().Be("e-9");
        result[0].User.Descriptor.Should().Be("aad.nine");
    }

    [Test]
    public async Task GetMemberships_returns_container_and_member_descriptors()
    {
        const string json = """{"count":1,"value":[{"containerDescriptor":"vssgp.c","memberDescriptor":"aad.m"}]}""";

        var result = await Create(StubHttpMessageHandler.Json(json)).GetMembershipsAsync("aad.m", Ct);

        var membership = result.Should().ContainSingle().Subject;
        membership.ContainerDescriptor.Should().Be("vssgp.c");
        membership.MemberDescriptor.Should().Be("aad.m");
    }

    [Test]
    public async Task MatchUsersWithGroupMemberships_maps_each_user_to_the_groups_it_belongs_to()
    {
        var ada = new UserDto { Descriptor = "aad.ada" };
        var bob = new UserDto { Descriptor = "aad.bob" };
        var admins = new GroupDto { Descriptor = "vssgp.admins" };
        var readers = new GroupDto { Descriptor = "vssgp.readers" };
        var handler = new StubHttpMessageHandler(r => r.Url.Contains("/aad.ada?")
            ? StubHttpMessageHandler.JsonResponse("""{"value":[{"containerDescriptor":"vssgp.admins"},{"containerDescriptor":"vssgp.unknown"}]}""")
            : StubHttpMessageHandler.JsonResponse("""{"value":[]}"""));

        var result = await Create(handler).MatchUsersWithGroupMembershipsAsync([ada, bob], [admins, readers], Ct);

        result.Keys.Should().BeEquivalentTo(["aad.ada", "aad.bob"]);
        result["aad.ada"].Should().Equal(admins);
        result["aad.bob"].Should().BeEmpty();
    }

    // ---- failure modes --------------------------------------------------------------------

    [TestCase(HttpStatusCode.Unauthorized)]
    [TestCase(HttpStatusCode.Forbidden)]
    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public async Task Failing_status_code_surfaces_as_HttpRequestException_carrying_that_status(HttpStatusCode status)
    {
        var sut = Create(StubHttpMessageHandler.Status(status));

        var act = () => sut.GetAllUsersAsync(Ct);

        (await act.Should().ThrowAsync<HttpRequestException>()).Which.StatusCode.Should().Be(status);
    }

    [Test]
    public async Task Unauthorized_inside_a_parallel_fan_out_keeps_its_status_code()
    {
        var sut = Create(StubHttpMessageHandler.Status(HttpStatusCode.Unauthorized));

        var act = () => sut.GetUserEntitlementsByUserIdsAsync(["a", "b", "c"], Ct);

        (await act.Should().ThrowAsync<HttpRequestException>()).Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Json_null_body_is_reported_as_SerializationException()
    {
        var sut = Create(StubHttpMessageHandler.Json("null"));

        await FluentActions.Awaiting(() => sut.GetAllUsersAsync(Ct)).Should().ThrowAsync<SerializationException>();
        await FluentActions.Awaiting(() => sut.GetAllGroupsAsync(Ct)).Should().ThrowAsync<SerializationException>();
        await FluentActions.Awaiting(() => sut.SearchUserEntitlementsAsync(Ct)).Should().ThrowAsync<SerializationException>();
        await FluentActions.Awaiting(() => sut.GetMembershipsAsync("d", Ct)).Should().ThrowAsync<SerializationException>();
        await FluentActions.Awaiting(() => sut.GetUserEntitlementsByUserIdAsync("d", Ct)).Should().ThrowAsync<SerializationException>();
        await FluentActions.Awaiting(() => sut.GetSessionTokenDetailsForPersonalAccessTokenAsync("d", null, Ct)).Should().ThrowAsync<SerializationException>();
    }

    [Test]
    public async Task Non_json_body_with_success_status_throws_JsonException()
    {
        // What Azure DevOps sends for an unauthenticated call when the sign-in redirect is not suppressed.
        var sut = Create(StubHttpMessageHandler.Json("<html>Sign in</html>", HttpStatusCode.NonAuthoritativeInformation));

        await FluentActions.Awaiting(() => sut.GetAllUsersAsync(Ct)).Should().ThrowAsync<JsonException>();
    }
}
