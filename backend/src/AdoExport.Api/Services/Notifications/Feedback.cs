namespace AdoExport.Api.Services.Notifications;

public sealed class Feedback
{
    private readonly string? _name;

    private readonly string? _email;

    private readonly string? _text;

    /// <summary>
    /// Gets or sets the audit id.
    /// </summary>
    public required string AuditId { get; init; }

    /// <summary>
    /// Gets or sets the optional name.
    /// </summary>
    public string? Name
    {
        get => _name ?? "Unknown";
        init => _name = value;
    }

    /// <summary>
    /// Gets or sets the optional mail address.
    /// </summary>
    public string? Email
    {
        get => _email ?? "Unknown";
        init => _email = value;
    }

    /// <summary>
    /// Gets or sets the feedback text.
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    public string Text
    {
        get => _text ?? throw new ArgumentException(nameof(_text));
        init => _text = string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("No text provided", nameof(value)) : value;
    }
}