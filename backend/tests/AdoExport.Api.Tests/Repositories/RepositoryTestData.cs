using AdoExport.Api.Services;
using AdoExport.Auditing;
using AdoExport.Auditing.Rules;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Api.Tests.Repositories;

/// <summary>
/// Small, explicit builders for the audit domain objects the repositories work on.
/// Every value that influences a grouping or a count is passed in, so a test reads as
/// "this input produces this number" without hidden defaults.
/// </summary>
internal static class RepositoryTestData
{
    public const string AuditKey = "audit-key";

    public static UserDto User(string descriptor, string displayName = "User", string? metaType = UserDto.MemberUserMetaType)
        => new()
        {
            Descriptor = descriptor,
            DisplayName = displayName,
            MetaType = metaType,
        };

    public static SessionTokenDto Token(
        string authorizationId,
        string userId,
        string displayName = "token",
        string validFrom = "2026-01-02T03:04:05",
        string validTo = "2026-07-08T09:10:11",
        string scope = "vso.code vso.build")
        => new()
        {
            AuthorizationId = authorizationId,
            UserId = userId,
            DisplayName = displayName,
            ValidFrom = validFrom,
            ValidTo = validTo,
            Scope = scope,
        };

    public static AccessLevelDto AccessLevel(string status) => new() { Status = status };

    public static AuditResult<T> ResultFor<T>(T auditee, params Severities[] severities)
        => new()
        {
            Auditee = auditee,
            RuleResults = severities.Select(severity => new RuleResult { Reason = new TestReason(Guid.NewGuid(), severity) }).ToArray(),
        };

    public static AuditResult<T> ResultFor<T>(T auditee, params RuleResult[] ruleResults)
        => new() { Auditee = auditee, RuleResults = ruleResults };

    public static RuleResult Rule(Guid reasonId, Severities severity, Categories category = Categories.Pats)
        => new() { Reason = new TestReason(reasonId, severity, category) };

    public static Audit Audit(
        AuditResult<SessionTokenDto>[]? pats = null,
        AuditResult<SessionTokenDto>[]? sshKeys = null,
        AuditResult<UserDto>[]? users = null)
        => new(new AuditMetadata
        {
            AuditId = Guid.NewGuid(),
            OrganizationName = "org",
            ExpiresAfter = TimeSpan.FromMinutes(30),
        })
        {
            PatAuditResults = pats ?? [],
            SshKeysAuditResults = sshKeys ?? [],
            UserAuditResults = users ?? [],
        };

    public static RawAuditData RawData(
        UserDto[]? allUsers = null,
        SessionTokenWithUser[]? privateTokens = null,
        SessionTokenWithUser[]? publicTokens = null,
        GetUserEntitlementsByUserEntitlementIdResponseDto[]? entitlements = null)
        => new()
        {
            UserGroupMemberships = new Dictionary<string, GroupDto[]>(),
            AllUsers = allUsers ?? [],
            PrivateSessionTokenDetailsWithUser = privateTokens ?? [],
            PublicSessionTokenDetailsWithUser = publicTokens ?? [],
            UsersWithProjectEntitlements = entitlements ?? [],
        };

    public static SessionTokenWithUser TokenWithUser(SessionTokenDto token, UserDto user)
        => new() { SessionToken = token, User = user };

    public static GetUserEntitlementsByUserEntitlementIdResponseDto Entitlement(UserDto user, AccessLevelDto? accessLevel)
        => new() { User = user, AccessLevel = accessLevel! };

    public static CreateAuditData CreateAuditData(RawAuditData? raw)
        => new()
        {
            Metadata = new AuditMetadata
            {
                AuditId = Guid.NewGuid(),
                OrganizationName = "org",
                ExpiresAfter = TimeSpan.FromMinutes(30),
            },
            ExportedData = raw,
        };

    /// <summary>
    /// Runs <paramref name="action"/> inside a scope in which <see cref="AuditKey"/> is the audit key,
    /// the way the request pipeline does it.
    /// </summary>
    public static async Task<T> InScopeAsync<T>(ScopedAuditKeyBearer bearer, Func<Task<T>> action)
        => await bearer.ScopedAsync(AuditKey, action);

    private sealed class TestReason : AuditRuleReason
    {
        public TestReason(Guid id, Severities severity, Categories category = Categories.Pats)
            : base(category)
        {
            Id = id;
            Severity = severity;
        }

        public override Guid Id { get; }
    }
}
