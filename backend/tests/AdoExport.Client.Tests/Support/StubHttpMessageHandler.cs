using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace AdoExport.Client.Tests.Support;

/// <summary>
/// An <see cref="HttpMessageHandler" /> that answers from a delegate and records what was sent.
/// </summary>
internal sealed class StubHttpMessageHandler(Func<RecordedRequest, HttpResponseMessage> responder) : HttpMessageHandler
{
    private readonly ConcurrentQueue<RecordedRequest> _requests = new();

    public IReadOnlyCollection<RecordedRequest> Requests => _requests;

    public static StubHttpMessageHandler Json(string json, HttpStatusCode status = HttpStatusCode.OK)
        => new(_ => JsonResponse(json, status));

    public static StubHttpMessageHandler Status(HttpStatusCode status)
        => new(_ => new HttpResponseMessage(status));

    public static HttpResponseMessage JsonResponse(string json, HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var recorded = new RecordedRequest(
            request.Method,
            request.RequestUri!.ToString(),
            request.Headers.Authorization?.Scheme,
            request.Headers.Authorization?.Parameter,
            request.Headers.Accept.Select(a => a.MediaType).ToArray(),
            request.Headers.UserAgent.ToString(),
            request.Headers.TryGetValues("X-TFS-FedAuthRedirect", out var values) ? string.Join(",", values) : null);

        _requests.Enqueue(recorded);

        return Task.FromResult(responder(recorded));
    }
}

internal sealed record RecordedRequest(
    HttpMethod Method,
    string Url,
    string? AuthScheme,
    string? AuthParameter,
    string?[] Accept,
    string UserAgent,
    string? FedAuthRedirect);
