using System.Net;

using AdoExport.Api.Controllers;
using AdoExport.Api.Repositories;
using AdoExport.Api.Tests.Controllers.Support;

using Microsoft.AspNetCore.Mvc;

namespace AdoExport.Api.Tests.Controllers;

public class PatsControllerTests : ControllerTestBase
{
    [Test]
    public async Task GetAllPats_ReturnsEveryPatOfTheAudit()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/pats?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.GetProperty("organizationName").GetString().Should().Be(TestAuditData.OrganizationName);
        json.GetProperty("validUntil").GetString().Should().StartWith("2026-11-12");

        var pats = json.GetProperty("pats").EnumerateArray().ToArray();
        pats.Select(pat => pat.GetProperty("id").GetString())
            .Should()
            .Equal(TestAuditData.FirstPatId, TestAuditData.SecondPatId);

        var first = pats[0];
        first.GetProperty("user").GetProperty("userName").GetString().Should().Be("Alice");
        first.GetProperty("scopes").EnumerateArray().Select(scope => scope.GetString())
            .Should()
            .Equal("vso.code", "vso.build");
        first.GetProperty("affectedPatRulesCounts").GetProperty("totalCriticals").GetInt32().Should().Be(1);
        first.GetProperty("affectedPatRulesCounts").GetProperty("totalWarnings").GetInt32().Should().Be(1);
    }

    [Test]
    public async Task GetAllPats_WithoutAuditId_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync("/api/pats");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Audit-Id is required");
    }

    [Test]
    public async Task GetAllPats_ForExpiredAudit_ReturnsGone()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/pats?auditId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Test]
    public async Task GetPatById_ReturnsThePatWithItsRulesMostSevereFirst()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/pats/{TestAuditData.FirstPatId}?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.GetProperty("pat").GetProperty("id").GetString().Should().Be(TestAuditData.FirstPatId);

        var rules = json.GetProperty("rules").EnumerateArray().ToArray();
        rules.Select(rule => rule.GetProperty("id").GetGuid())
            .Should()
            .Equal(TestAuditData.PatAppScopeRuleId, TestAuditData.PatExpiresOverQuarterRuleId);

        // The count is across the whole audit, not just this PAT: both PATs have the app scope.
        rules[0].GetProperty("totalCounts").GetInt32().Should().Be(2);
        rules[1].GetProperty("totalCounts").GetInt32().Should().Be(1);
    }

    [Test]
    public async Task GetPatById_ForUnknownPat_ReturnsNotFound()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/pats/does-not-exist?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadAsStringAsync()).Should().Contain("PAT not found");
    }

    [Test]
    public async Task GetPatById_WithoutAuditId_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/pats/{TestAuditData.FirstPatId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetPatById_ForExpiredAudit_ReturnsGone()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/pats/{TestAuditData.FirstPatId}?auditId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Test]
    public async Task GetPatById_WithBlankPatId_ReturnsBadRequest()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/pats/%20?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Model binding rejects a blank route value before the action runs, so this guard is
    // only reachable by calling the action directly.
    [Test]
    public async Task GetPatById_WhenCalledDirectlyWithBlankPatId_ReturnsBadRequest()
    {
        var controller = new PatsController(
            Mock.Of<IUserRepository>(),
            Mock.Of<IAuditRepository>(),
            Mock.Of<IRulesRepository>(),
            Mock.Of<IPatsFacade>());

        var result = await controller.GetPatDetailsByPatId(" ", Guid.NewGuid(), null!);

        result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be("Pat-Id is required");
    }
}
