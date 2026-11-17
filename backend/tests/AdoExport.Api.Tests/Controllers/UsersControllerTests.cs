using System.Net;

using AdoExport.Api.Controllers;
using AdoExport.Api.Repositories;
using AdoExport.Api.Tests.Controllers.Support;

using Microsoft.AspNetCore.Mvc;

namespace AdoExport.Api.Tests.Controllers;

public class UsersControllerTests : ControllerTestBase
{
    [Test]
    public async Task GetAllUsers_ListsUsersWithAnAccessLevelAndSkipsTheOthers()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/users?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        var users = json.GetProperty("users").EnumerateArray().ToArray();

        // Carol exists in the organization but has no access level, so she is left out.
        users.Select(user => user.GetProperty("userName").GetString())
            .Should()
            .Equal("Alice", "Bob");
        users[0].GetProperty("isActive").GetBoolean().Should().BeTrue();
        users[0].GetProperty("affectedUserRulesCounts").GetProperty("totalWarnings").GetInt32().Should().Be(1);
        users[1].GetProperty("isActive").GetBoolean().Should().BeFalse();
    }

    [Test]
    public async Task GetAllUsers_WithoutAuditId_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Audit-Id is required");
    }

    [Test]
    public async Task GetAllUsers_ForExpiredAudit_ReturnsGone()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/users?auditId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Test]
    public async Task GetUserDetails_ReturnsOnlyThePatsOwnedByThatUser()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/users/{TestAuditData.FirstUserDescriptor}?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.GetProperty("user").GetProperty("userName").GetString().Should().Be("Alice");

        json.GetProperty("pats").EnumerateArray().Select(pat => pat.GetProperty("id").GetString())
            .Should()
            .Equal(TestAuditData.FirstPatId);
        json.GetProperty("patsSummary").GetProperty("totalCriticals").GetInt32().Should().Be(1);
        json.GetProperty("rules").EnumerateArray().Select(rule => rule.GetProperty("id").GetGuid())
            .Should()
            .Equal(TestAuditData.UserIsAdminRuleId);
    }

    [Test]
    public async Task GetUserDetails_ForUnknownUser_ReturnsNotFound()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/users/aad.nobody?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadAsStringAsync()).Should().Contain("User not found");
    }

    [Test]
    public async Task GetUserDetails_ForUserWithoutAccessLevel_ReturnsNotFound()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/users/{TestAuditData.UserWithoutAccessLevelDescriptor}?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Access level not found");
    }

    [Test]
    public async Task GetUserDetails_WithoutAuditId_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/users/{TestAuditData.FirstUserDescriptor}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetUserDetails_WithBlankUserId_ReturnsBadRequest()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/users/%20?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetUserDetails_ForExpiredAudit_ReturnsGone()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/users/{TestAuditData.FirstUserDescriptor}?auditId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    // Model binding rejects a blank route value before the action runs, so this guard is
    // only reachable by calling the action directly.
    [Test]
    public async Task GetUserDetails_WhenCalledDirectlyWithBlankUserId_ReturnsBadRequest()
    {
        var controller = new UsersController(Mock.Of<IUsersFacade>());

        var result = await controller.GetUserDetailsByUserId(" ", Guid.NewGuid(), null!);

        result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be("User-Id is required");
    }
}
