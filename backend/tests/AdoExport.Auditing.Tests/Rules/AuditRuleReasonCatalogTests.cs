using AdoExport.Auditing.Rules;
using AdoExport.Auditing.Rules.Pats;
using AdoExport.Auditing.Rules.Pats.Reasons;
using AdoExport.Auditing.Rules.SshKeys;
using AdoExport.Auditing.Rules.SshKeys.Reasons;
using AdoExport.Auditing.Rules.Users;
using AdoExport.Auditing.Rules.Users.Reasons;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Tests.Rules;

/// <summary>
/// Pins identity, category, severity and model metadata of every reason. The ids are what consumers
/// (hints, frontend) key on, and the severity is what decides whether an organisation looks healthy,
/// so a change to either must be a deliberate edit of this table.
/// </summary>
[TestFixture]
public class AuditRuleReasonCatalogTests
{
    public sealed record ReasonExpectation(Type ReasonType, Guid Id, Categories Category, Severities Severity, Type ModelType);

    private static TestCaseData Case<TReason>(string id, Categories category, Severities severity, Type modelType)
    {
        return new TestCaseData(new ReasonExpectation(typeof(TReason), Guid.Parse(id), category, severity, modelType))
            .SetName($"{typeof(TReason).Name} is {severity} in {category}");
    }

    private static IEnumerable<TestCaseData> Expectations()
    {
        yield return Case<PatExpiresInOverHalfAYearReason>("05f06a6b-5f3d-4f16-bb63-7e035c962f48", Categories.Pats, Severities.Critical, typeof(SessionTokenDto));
        yield return Case<PatExpiresInOverOneQuarterReason>("27bd636c-6a92-4c77-9023-d91d3a0dc129", Categories.Pats, Severities.Warning, typeof(SessionTokenDto));
        yield return Case<PatExpiresWithinShortTimeRangeReason>("1529f5ce-6264-4296-8d54-605e68836317", Categories.Pats, Severities.Ok, typeof(SessionTokenDto));
        yield return Case<PatHasAppScopeReason>("5a5adb71-e8ac-40d3-8956-15e6465be028", Categories.Pats, Severities.Critical, typeof(SessionTokenDto));
        yield return Case<PatHasLimitedScopeReason>("0b705033-2e32-4ce3-b606-7dceb31aee96", Categories.Pats, Severities.Ok, typeof(SessionTokenDto));
        yield return Case<PatIsValidForAllOrganizationsReason>("e6a47ea8-34f1-43c5-b50c-df0ebda16cfc", Categories.Pats, Severities.Critical, typeof(SessionTokenDto));
        yield return Case<PatIsValidForMoreThanOneOrganizationReason>("cf7dcc01-8db3-41a6-b6b9-a6b856d72c76", Categories.Pats, Severities.Warning, typeof(SessionTokenDto));
        yield return Case<PatIsValidForOnlyOneOrganizationReason>("f7084c80-0444-4525-bf8c-f8c58c07b026", Categories.Pats, Severities.Ok, typeof(SessionTokenDto));
        yield return Case<SshKeyExpiresInOverAYearReason>("cb5666b5-fc99-4994-9ce3-b464bff65cf7", Categories.Pats, Severities.Critical, typeof(SessionTokenDto));
        yield return Case<SshKeyExpiresInOverHalfAYearReason>("20922ad1-012e-4c6a-8b1d-4c712a819679", Categories.Pats, Severities.Warning, typeof(SessionTokenDto));
        yield return Case<SshKeyExpiresWithinShortTimeRangeReason>("de414e9c-ae7e-4498-a12f-68ce12ec4efc", Categories.Pats, Severities.Ok, typeof(SessionTokenDto));
        yield return Case<SshKeyIsValidForAllOrganizationsReason>("3bd706aa-f35d-4c68-8a76-90398a0c5860", Categories.Pats, Severities.Critical, typeof(SessionTokenDto));
        yield return Case<SshKeyIsValidForMoreThanOneOrganizationReason>("0919cce4-52f0-429e-804d-72227583d3c9", Categories.Pats, Severities.Warning, typeof(SessionTokenDto));
        yield return Case<SshKeyIsValidForOnlyOneOrganizationReason>("41616316-59e1-4198-98de-90247be45073", Categories.Pats, Severities.Ok, typeof(SessionTokenDto));
        yield return Case<UserExceedsOverallPatLimitReason>("4b327b8a-8f83-44f7-85bd-e05b98b5fedd", Categories.Users, Severities.Warning, typeof(UserDto));
        yield return Case<UserIsNotOrganizationAdminReason>("14cf0a21-aa67-4a11-8cea-abf0bfaec4b1", Categories.Users, Severities.Ok, typeof(UserDto));
        yield return Case<UserIsOfTypeAgentPoolReason>("4b5d1e5f-49f6-4151-a50e-92ce326b79c3", Categories.Users, Severities.Ok, typeof(UserDto));
        yield return Case<UserIsOfTypeBuildAgentReason>("962a4349-6dea-4e0f-ac10-8b6ebd8a9be6", Categories.Users, Severities.Ok, typeof(UserDto));
        yield return Case<UserIsOfTypeGuestReason>("459ddf63-91b0-4722-89f7-548f19b36799", Categories.Users, Severities.Warning, typeof(UserDto));
        yield return Case<UserIsOfTypeMemberReason>("498a578a-110e-4cd6-ba80-ba641171420a", Categories.Users, Severities.Ok, typeof(UserDto));
        yield return Case<UserIsOfTypeUnknownReason>("d1ce9fd4-50c1-4899-8a5d-4120acbf3d54", Categories.Users, Severities.Critical, typeof(UserDto));
        yield return Case<UserIsOrganizationAdminReason>("d9883edd-8181-4acb-9e13-b44e04d2ca7d", Categories.Users, Severities.Warning, typeof(UserDto));
        yield return Case<UserIsWithinOverallPatLimitReason>("37c18e21-67ed-4abf-9471-73cbcf743138", Categories.Users, Severities.Ok, typeof(UserDto));
    }

    private static AuditRuleReason Create(ReasonExpectation expectation, object model)
    {
        return (AuditRuleReason)Activator.CreateInstance(expectation.ReasonType, model)!;
    }

    private static object CreateModel(Type modelType)
    {
        return modelType == typeof(UserDto) ? new UserDto() : new SessionTokenDto();
    }

    [TestCaseSource(nameof(Expectations))]
    public void Reason_HasExpectedIdCategoryAndSeverity(ReasonExpectation expectation)
    {
        var reason = Create(expectation, CreateModel(expectation.ModelType));

        reason.Id.Should().Be(expectation.Id);
        reason.Category.Should().Be(expectation.Category);
        reason.Severity.Should().Be(expectation.Severity);
    }

    [TestCaseSource(nameof(Expectations))]
    public void Reason_CarriesTheAuditeeAsModelMetadata(ReasonExpectation expectation)
    {
        var model = CreateModel(expectation.ModelType);

        var reason = Create(expectation, model);

        reason.Metadata.Should().ContainKey(AuditRuleReason.ModelMetadataKey);
        reason.Metadata[AuditRuleReason.ModelMetadataKey].Should().BeSameAs(model);
    }

    [Test]
    public void Reasons_HaveUniqueIds()
    {
        var ids = Expectations()
            .Select(testCase => ((ReasonExpectation)testCase.Arguments[0]!).Id)
            .ToArray();

        ids.Should().OnlyHaveUniqueItems();
        ids.Should().NotContain(Guid.Empty);
    }

    [Test]
    public void Catalog_ContainsEveryConcreteReasonInTheAssembly()
    {
        var catalogued = Expectations()
            .Select(testCase => ((ReasonExpectation)testCase.Arguments[0]!).ReasonType)
            .ToHashSet();

        var concreteReasons = typeof(AuditRuleReason).Assembly
            .GetTypes()
            .Where(type => type is { IsAbstract: false } && type.IsSubclassOf(typeof(AuditRuleReason)))
            .ToArray();

        concreteReasons.Should().BeEquivalentTo(catalogued, "a new reason must be added to this catalog");
    }
}
