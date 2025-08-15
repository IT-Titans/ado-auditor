namespace AdoExport.Api.Services;

/// <summary>
/// Used to make the audit key available for the services scoped to a single request.
/// </summary>
public sealed class ScopedAuditKeyBearer
{
    private string? _auditKey;

    /// <summary>
    /// The audit key to identify / access a single audit.
    /// </summary>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ArgumentException" />
    public string AuditKey
    {
        get => _auditKey ?? throw new InvalidOperationException("No audit key has been configured");
        private set => _auditKey = value ?? throw new ArgumentException("A valid audit key needs to be provided", nameof(value));
    }

    /// <summary>
    /// Creates an async scope in which the <paramref name="auditKey" /> is accessible for the <paramref name="scopedAction" />.<br />
    /// After <paramref name="scopedAction" /> completes, the <paramref name="auditKey" /> is no longer accessible.
    /// </summary>
    /// <param name="auditKey">The audit key.</param>
    /// <param name="scopedAction">The action that needs the audit key.</param>
    /// <typeparam name="TResult">The <paramref name="scopedAction" />s return type.</typeparam>
    /// <returns>An awaitable task that completes when <paramref name="scopedAction" /> completes.</returns>
    public async Task<TResult> ScopedAsync<TResult>(string auditKey, Func<Task<TResult>> scopedAction)
    {
        Guard.IsNotNullOrWhiteSpace(auditKey);
        Guard.IsNotNull(scopedAction);

        try
        {
            AuditKey = auditKey;

            return await scopedAction().ConfigureAwait(false);
        }
        finally
        {
            ClearAuditKey();
        }
    }

    /// <summary>
    /// Creates an async scope in which the <paramref name="auditKey" /> is accessible for the <paramref name="scopedAction" />.<br />
    /// After <paramref name="scopedAction" /> completes, the <paramref name="auditKey" /> is no longer accessible.
    /// </summary>
    /// <param name="auditKey">The audit key.</param>
    /// <param name="scopedAction">The action that needs the audit key.</param>
    /// <returns>An awaitable task that completes when <paramref name="scopedAction" /> completes.</returns>
    public async Task ScopedAsync(string auditKey, Func<Task> scopedAction)
    {
        Guard.IsNotNullOrWhiteSpace(auditKey);
        Guard.IsNotNull(scopedAction);

        try
        {
            AuditKey = auditKey;

            await scopedAction().ConfigureAwait(false);
        }
        finally
        {
            ClearAuditKey();
        }
    }

    private void ClearAuditKey() => _auditKey = null;
}