using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace AdoExport.Api.Services.Notifications;

public sealed class AuditMetrics
{
    private readonly Dictionary<string, string?> _metadata = new Dictionary<string, string?>();

    public AuditMetrics Add(string key, string? value)
    {
        Guard.IsNotNullOrWhiteSpace(key);

        _metadata.Add(key, value);

        return this;
    }

    /// <summary>
    /// Gets or sets the audit identifier.
    /// </summary>
    [JsonPropertyName("auditIdentifier")]
    public required string AuditId { get; init; }

    /// <summary>
    /// Gets or sets the audit log metadata.
    /// </summary>
    [JsonPropertyName("metadata")]
    public ImmutableDictionary<string, string?> Metadata => _metadata.ToImmutableDictionary();
}