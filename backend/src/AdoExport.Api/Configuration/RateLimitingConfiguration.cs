namespace AdoExport.Api.Configuration;

/// <summary>
/// Limits for the request rate a single client may produce.
/// </summary>
/// <remarks>
/// The service has no accounts, so a client is identified by its IP address and nothing
/// else. That is a weak identity - a determined caller changes it - but it is the only
/// one available, and it still raises the cost of the cases that matter here: flooding
/// the contact and feedback forms, and starting audits in a loop.
/// </remarks>
public class RateLimitingConfiguration
{
    public const string ConfigurationSectionName = "RateLimiting";

    /// <summary>
    /// Gets or sets a value indicating whether rate limiting is enforced at all.
    /// </summary>
    /// <remarks>
    /// Exists so a load test or a local run can switch it off deliberately. It defaults
    /// to on, so forgetting to configure the section leaves the limits in place rather
    /// than removing them.
    /// </remarks>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the window over which every limit below is counted.
    /// </summary>
    public TimeSpan Window { get; init; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Gets or sets how many requests one client may send to the mail-sending endpoints
    /// per <see cref="Window" />.
    /// </summary>
    /// <remarks>
    /// Each accepted request puts a mail into an IT Titans mailbox, so this is the
    /// tightest limit of the three.
    /// </remarks>
    public int MailPermitLimit { get; init; } = 3;

    /// <summary>
    /// Gets or sets how many audits one client may start per <see cref="Window" />.
    /// </summary>
    /// <remarks>
    /// An audit calls the Azure DevOps API repeatedly and sends a consent mail, so it is
    /// far more expensive than reading a result that already exists.
    /// </remarks>
    public int AuditPermitLimit { get; init; } = 5;

    /// <summary>
    /// Gets or sets how many requests one client may send to every other endpoint per
    /// <see cref="Window" />.
    /// </summary>
    /// <remarks>
    /// Reading a finished audit is cheap and the frontend does it repeatedly while a user
    /// works through the result, so this limit is generous on purpose.
    /// </remarks>
    public int GlobalPermitLimit { get; init; } = 300;

    /// <summary>
    /// Gets or sets how many requests may wait for a free slot instead of being rejected
    /// immediately.
    /// </summary>
    public int QueueLimit { get; init; }

    /// <summary>
    /// Gets or sets the secret the frontend server presents so that the visitor address
    /// it passes on is believed.
    /// </summary>
    /// <remarks>
    /// Browsers reach this API through the frontend server only, so without this every
    /// visitor is counted under the frontend's own address and they all share one budget.
    /// Leaving it empty switches the mechanism off: the visitor address header is then
    /// ignored and the socket address is used, which is the behaviour before the setting
    /// existed. The frontend holds the same value as <c>NUXT_PROXY_KEY</c>.
    /// </remarks>
    public string? TrustedProxyKey { get; init; }
}
