using System.Net;
using System.Text.Json;

using AdoExport.Api.Tests.Controllers.Support;

namespace AdoExport.Api.Tests.Controllers;

public class RulesControllerTests : ControllerTestBase
{
    private static Guid[] RuleIds(JsonElement json)
        => json.GetProperty("rules").EnumerateArray().Select(rule => rule.GetProperty("id").GetGuid()).ToArray();

    // --- GET /api/rules/pats ---

    [Test]
    public async Task GetPatRules_WithoutSeverity_ReturnsAllRulesMostSevereFirst()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/pats?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        json.GetProperty("organizationName").GetString().Should().Be(TestAuditData.OrganizationName);
        RuleIds(json).Should().Equal(TestAuditData.PatAppScopeRuleId, TestAuditData.PatExpiresOverQuarterRuleId);
        json.GetProperty("rules")[0].GetProperty("totalCounts").GetInt32().Should().Be(2);
    }

    [Test]
    public async Task GetPatRules_FilteredBySeverity_ReturnsOnlyMatchingRules()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/pats?auditId={auditId}&severity=warning");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RuleIds(await ReadJsonAsync(response)).Should().Equal(TestAuditData.PatExpiresOverQuarterRuleId);
    }

    [Test]
    public async Task GetPatRules_WithSeverityMatchingNothing_ReturnsEmptyList()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/pats?auditId={auditId}&severity=ok");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RuleIds(await ReadJsonAsync(response)).Should().BeEmpty();
    }

    [Test]
    public async Task GetPatRules_WithUnknownSeverity_ReturnsBadRequest()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/pats?auditId={auditId}&severity=catastrophic");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetPatRules_WithoutAuditId_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync("/api/rules/pats");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Audit-Id is required");
    }

    [Test]
    public async Task GetPatRules_ForExpiredAudit_ReturnsGone()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/pats?auditId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    // --- GET /api/rules/pats/{ruleId} ---

    [Test]
    public async Task GetPatsByRule_ReturnsEveryPatTheRuleAppliesTo()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/pats/{TestAuditData.PatAppScopeRuleId}?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rule = (await ReadJsonAsync(response)).GetProperty("rule");
        rule.GetProperty("id").GetGuid().Should().Be(TestAuditData.PatAppScopeRuleId);
        rule.GetProperty("totalCounts").GetInt32().Should().Be(2);
        rule.GetProperty("severity").GetString().Should().Be("critical");
        rule.GetProperty("name").GetString().Should().NotBeNullOrWhiteSpace();

        // The PAT with more findings (critical and warning) precedes the one with a single finding.
        rule.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("id").GetString())
            .Should()
            .Equal(TestAuditData.FirstPatId, TestAuditData.SecondPatId);
    }

    [Test]
    public async Task GetPatsByRule_ForRuleThatAppliesToOnePat_ReturnsOnlyThatPat()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/pats/{TestAuditData.PatExpiresOverQuarterRuleId}?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rule = (await ReadJsonAsync(response)).GetProperty("rule");
        rule.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("id").GetString())
            .Should()
            .Equal(TestAuditData.FirstPatId);
    }

    [Test]
    public async Task GetPatsByRule_ForRuleThatDoesNotExist_ReturnsNotFound()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/pats/{Guid.NewGuid()}?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Rule not found");
    }

    [Test]
    public async Task GetPatsByRule_WithEmptyRuleId_ReturnsBadRequest()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/pats/{Guid.Empty}?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Rule-Id is required");
    }

    [Test]
    public async Task GetPatsByRule_WithoutAuditId_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/pats/{TestAuditData.PatAppScopeRuleId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Audit-Id is required");
    }

    [Test]
    public async Task GetPatsByRule_ForExpiredAudit_ReturnsGone()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/pats/{TestAuditData.PatAppScopeRuleId}?auditId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    // --- GET /api/rules/users ---

    [Test]
    public async Task GetUserRules_WithoutSeverity_ReturnsAllRulesMostSevereFirst()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/users?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var ids = RuleIds(await ReadJsonAsync(response));
        ids.Should().HaveCount(2);
        ids[0].Should().Be(TestAuditData.UserIsAdminRuleId, "a warning precedes an ok");
    }

    [Test]
    public async Task GetUserRules_FilteredBySeverity_ReturnsOnlyMatchingRules()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/users?auditId={auditId}&severity=warning");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RuleIds(await ReadJsonAsync(response)).Should().Equal(TestAuditData.UserIsAdminRuleId);
    }

    [Test]
    public async Task GetUserRules_WithSeverityMatchingNothing_ReturnsEmptyList()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/users?auditId={auditId}&severity=critical");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RuleIds(await ReadJsonAsync(response)).Should().BeEmpty();
    }

    [Test]
    public async Task GetUserRules_WithoutAuditId_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync("/api/rules/users");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetUserRules_ForExpiredAudit_ReturnsGone()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/users?auditId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    // --- GET /api/rules/users/{ruleId} ---

    [Test]
    public async Task GetUsersByRule_ReturnsTheUsersTheRuleAppliesTo()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/users/{TestAuditData.UserIsAdminRuleId}?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rule = (await ReadJsonAsync(response)).GetProperty("rule");
        rule.GetProperty("id").GetGuid().Should().Be(TestAuditData.UserIsAdminRuleId);
        rule.GetProperty("totalCounts").GetInt32().Should().Be(1);
        rule.GetProperty("severity").GetString().Should().Be("warning");

        var items = rule.GetProperty("items").EnumerateArray().ToArray();
        items.Should().ContainSingle();
        items[0].GetProperty("id").GetString().Should().Be(TestAuditData.FirstUserDescriptor);
        items[0].GetProperty("userName").GetString().Should().Be("Alice");
        items[0].GetProperty("metaType").GetString().Should().Be("member");
    }

    [Test]
    public async Task GetUsersByRule_ForRuleThatDoesNotExist_ReturnsNotFound()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/users/{Guid.NewGuid()}?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Rule not found");
    }

    [Test]
    public async Task GetUsersByRule_WithEmptyRuleId_ReturnsBadRequest()
    {
        var auditId = await Factory.SeedAuditAsync();
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/users/{Guid.Empty}?auditId={auditId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Rule-Id is required");
    }

    [Test]
    public async Task GetUsersByRule_WithoutAuditId_ReturnsBadRequest()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/users/{TestAuditData.UserIsAdminRuleId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Audit-Id is required");
    }

    [Test]
    public async Task GetUsersByRule_ForExpiredAudit_ReturnsGone()
    {
        using var client = Factory.CreateUniqueClient();

        var response = await client.GetAsync($"/api/rules/users/{TestAuditData.UserIsAdminRuleId}?auditId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }
}
