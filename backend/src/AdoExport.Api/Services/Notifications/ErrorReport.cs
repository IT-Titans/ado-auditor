namespace AdoExport.Api.Services.Notifications;

public sealed class ErrorReport
{
    /// <summary>
    /// Gets or sets a unique identifier to represent this request in trace logs.
    /// </summary>
    public required string TraceId { get; init; }

    /// <summary>
    /// The error message that explains the reason for the exception, or an empty string ("").
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// A string that describes the immediate frames of the call stack. If no stack trace is available (such as prior to stack unwinding from a throw statement), the value is null.
    /// </summary>
    public required string? StackTrace { get; init; }
}