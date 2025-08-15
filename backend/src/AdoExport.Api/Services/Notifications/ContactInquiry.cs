namespace AdoExport.Api.Services.Notifications;

public sealed class ContactInquiry
{
    private readonly string? _name;

    private readonly string? _phoneNumber;

    private readonly string? _email;

    private readonly string? _text;

    /// <summary>
    /// Gets or sets the optional name.
    /// </summary>
    public string? Name
    {
        get => _name ?? "Unknown";
        init => _name = value;
    }

    /// <summary>
    /// Gets or sets the optional phone number.
    /// </summary>
    public string? PhoneNumber
    {
        get => _phoneNumber ?? "Unknown";
        init => _phoneNumber = value;
    }

    /// <summary>
    /// Gets or sets the optional email.
    /// </summary>
    public required string Email
    {
        get => _email ?? throw new ArgumentException("No email was provided", nameof(_email));
        init => _email = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("No email provided", nameof(value))
            : value;
    }

    /// <summary>
    /// Gets or sets the feedback text.
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    public string Text
    {
        get => _text ?? throw new ArgumentException(nameof(_text));
        init => _text = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("No text provided", nameof(value))
            : value;
    }
}