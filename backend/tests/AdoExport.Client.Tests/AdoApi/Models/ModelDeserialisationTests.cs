using System.Text.Json;

using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Client.Tests.AdoApi.Models;

public class ModelDeserialisationTests
{
    [Test]
    public void UserDto_maps_wire_names()
    {
        const string json = """
            {"subjectKind":"user","metaType":"guest","directoryAlias":"ada","domain":"d","principalName":"ada@x",
             "mailAddress":"ada@x.test","origin":"aad","originId":"o-1","displayName":"Ada","url":"https://u","descriptor":"aad.AAAA"}
            """;

        var user = JsonSerializer.Deserialize<UserDto>(json)!;

        user.SubjectKind.Should().Be("user");
        user.MetaType.Should().Be("guest");
        user.DirectoryAlias.Should().Be("ada");
        user.Domain.Should().Be("d");
        user.PrincipalName.Should().Be("ada@x");
        user.MailAddress.Should().Be("ada@x.test");
        user.Origin.Should().Be("aad");
        user.OriginId.Should().Be("o-1");
        user.DisplayName.Should().Be("Ada");
        user.Url.Should().Be("https://u");
        user.Descriptor.Should().Be("aad.AAAA");
    }

    [Test]
    public void UserDto_without_metaType_deserialises_to_null_and_displays_as_unknown()
    {
        var user = JsonSerializer.Deserialize<UserDto>("""{"descriptor":"aad.AAAA"}""")!;

        user.MetaType.Should().BeNull();
        user.DisplayableMetaType().Should().Be("unknown");
    }

    [Test]
    public void GroupDto_maps_wire_names_including_nested_links()
    {
        const string json = """
            {"subjectKind":"group","description":"desc","domain":"d","principalName":"[p]\\Admins","mailAddress":null,
             "origin":"vsts","originId":"o-2","displayName":"Admins","url":"https://g","descriptor":"vssgp.AAAA",
             "_links":{"self":{"href":"h1"},"memberships":{"href":"h2"},"membershipState":{"href":"h3"},"storageKey":{"href":"h4"}}}
            """;

        var group = JsonSerializer.Deserialize<GroupDto>(json)!;

        group.DisplayName.Should().Be("Admins");
        group.Description.Should().Be("desc");
        group.PrincipalName.Should().Be("[p]\\Admins");
        group.MailAddress.Should().BeNull();
        group.Origin.Should().Be("vsts");
        group.OriginId.Should().Be("o-2");
        group.Url.Should().Be("https://g");
        group.Links.Self.Href.Should().Be("h1");
        group.Links.Memberships.Href.Should().Be("h2");
        group.Links.MembershipState.Href.Should().Be("h3");
        group.Links.StorageKey.Href.Should().Be("h4");
    }

    [Test]
    public void ProjectEntitlementsDto_maps_all_fields()
    {
        const string json = """
            {"projectRef":{"id":"p1","name":"Alpha"},"group":{"groupType":"projectAdministrator","displayName":"Admins"},
             "projectPermissionInherited":"notSet","assignmentSource":"unknown"}
            """;

        var entitlement = JsonSerializer.Deserialize<ProjectEntitlementsDto>(json)!;

        entitlement.ProjectReference.Id.Should().Be("p1");
        entitlement.Group.DisplayName.Should().Be("Admins");
        entitlement.ProjectPermissionInherited.Should().Be("notSet");
        entitlement.AssignmentSource.Should().Be("unknown");
    }

    [Test]
    public void AccessLevelDto_maps_all_fields()
    {
        const string json = """
            {"licensingSource":"account","accountLicenseType":"express","msdnLicenseType":"none","gitHubLicenseType":"none",
             "licenseDisplayName":"Basic","status":"active","statusMessage":"ok","assignmentSource":"unknown"}
            """;

        var level = JsonSerializer.Deserialize<AccessLevelDto>(json)!;

        level.LicensingSource.Should().Be("account");
        level.AccountLicenseType.Should().Be("express");
        level.MsdnLicenseType.Should().Be("none");
        level.GitHubLicenseType.Should().Be("none");
        level.LicenseDisplayName.Should().Be("Basic");
        level.Status.Should().Be("active");
        level.StatusMessage.Should().Be("ok");
        level.AssignmentSource.Should().Be("unknown");
    }

    [Test]
    public void SessionTokenDto_maps_all_fields()
    {
        const string json = """
            {"clientId":"c","accessId":"a","authorizationId":"az","hostAuthorizationId":"h","userId":"u",
             "validFrom":"2026-01-01T00:00:00Z","validTo":"2027-01-01T00:00:00Z","displayName":"ci","scope":"vso.code",
             "targetAccounts":["t1","t2"],"token":null,"alternateToken":null,"isValid":true,"isPublic":true,
             "publicData":null,"source":null,"claims":null}
            """;

        var token = JsonSerializer.Deserialize<SessionTokenDto>(json)!;

        token.ClientId.Should().Be("c");
        token.AccessId.Should().Be("a");
        token.AuthorizationId.Should().Be("az");
        token.HostAuthorizationId.Should().Be("h");
        token.UserId.Should().Be("u");
        token.ValidFrom.Should().Be("2026-01-01T00:00:00Z");
        token.ValidTo.Should().Be("2027-01-01T00:00:00Z");
        token.DisplayName.Should().Be("ci");
        token.Scope.Should().Be("vso.code");
        token.TargetAccounts.Should().Equal("t1", "t2");
        token.IsValid.Should().BeTrue();
        token.IsPublic.Should().BeTrue();
    }

    [Test]
    public void SessionTokenDto_tolerates_null_targetAccounts()
    {
        var token = JsonSerializer.Deserialize<SessionTokenDto>("""{"targetAccounts":null,"isValid":false}""")!;

        token.TargetAccounts.Should().BeNull();
        token.IsValid.Should().BeFalse();
    }

    [Test]
    public void Unknown_properties_in_the_response_are_ignored()
    {
        var response = JsonSerializer.Deserialize<GetAllUsersResponseDto>(
            """{"count":0,"value":[],"somethingNewInApiVersion8":{"a":1}}""")!;

        response.Value.Should().BeEmpty();
    }

    [Test]
    public void Response_envelopes_expose_count_and_continuation_token()
    {
        JsonSerializer.Deserialize<GetAllUsersResponseDto>("""{"count":2,"continuationToken":"x","value":[]}""")!
            .Should().Match<GetAllUsersResponseDto>(r => r.Count == 2 && r.ContinuationToken == "x");
        JsonSerializer.Deserialize<GetAllGroupsResponseDto>("""{"count":3,"continuationToken":"y","value":[]}""")!
            .Should().Match<GetAllGroupsResponseDto>(r => r.Count == 3 && r.ContinuationToken == "y");
        JsonSerializer.Deserialize<GetAllUserEntitlementsResponseDto>("""{"count":4,"value":[]}""")!
            .Count.Should().Be(4);
        JsonSerializer.Deserialize<GetSubjectMembershipsResponseDto>("""{"count":5,"value":[]}""")!
            .Count.Should().Be(5);
    }

    [Test]
    public void Envelope_without_a_value_array_leaves_Value_null()
    {
        // Documents the hazard behind the #nullable disable in the DTOs: nothing guarantees Value is set.
        JsonSerializer.Deserialize<GetAllUsersResponseDto>("""{"count":0}""")!.Value.Should().BeNull();
    }

    [TestCase("member", ExpectedResult = true)]
    [TestCase("GUEST", ExpectedResult = true)]
    [TestCase("agent", ExpectedResult = false)]
    [TestCase(null, ExpectedResult = false)]
    public bool IsRealUserType_accepts_only_member_and_guest(string? metaType)
        => new UserDto { MetaType = metaType }.IsRealUserType();

    [Test]
    public void Extensions_reject_null_receivers()
    {
        FluentActions.Invoking(() => ((UserDto)null!).DisplayableMetaType()).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => ((UserDto)null!).IsRealUserType()).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => ((AccessLevelDto)null!).IsActive()).Should().Throw<ArgumentNullException>();
    }
}
