using System.Text.Json;

namespace AdoExport.Api.Middlewares;

public class ErrorDetails
{
    /// <summary>
    /// Gets or sets a unique identifier to represent this request in trace logs.
    /// </summary>
    public required string TraceId { get; init; }

    /// <summary>
    /// Gets or sets the HTTP response code.
    /// </summary>
    public required int StatusCode { get; init; }

    /// <summary>
    /// The error message that explains the status code.
    /// </summary>
    public required string Message { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}