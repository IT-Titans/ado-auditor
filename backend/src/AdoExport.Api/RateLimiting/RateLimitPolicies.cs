namespace AdoExport.Api.RateLimiting;

/// <summary>
/// The names under which the rate limiting policies are registered.
/// </summary>
/// <remarks>
/// <c>EnableRateLimiting</c> takes a string. A typo there does not fail the build, it
/// throws at the first request that reaches the endpoint, so the names live here and
/// every attribute refers to one of these constants.
/// </remarks>
public static class RateLimitPolicies
{
    /// <summary>
    /// For endpoints where an accepted request puts a mail into an IT Titans mailbox.
    /// </summary>
    public const string Mail = "mail";

    /// <summary>
    /// For starting an audit, which calls the Azure DevOps API repeatedly on the
    /// caller's behalf.
    /// </summary>
    public const string Audit = "audit";
}
