using System.Net;
using System.Net.Http.Json;

namespace AdoExport.Api.Tests.RateLimiting;

/// <summary>
/// Exercises the contact endpoint through the real pipeline.
/// </summary>
/// <remarks>
/// The endpoint puts a mail into an IT Titans mailbox on every accepted request and has
/// no account behind it, so an unlimited caller is a mail flood waiting to happen. These
/// cases prove the limit is reached, that it is per client rather than global, and that
/// the client is recognised through the header Azure App Service forwards.
/// </remarks>
public class ContactRateLimitTests
{
    private const int PermitLimit = 3;

    private static object ValidInquiry() => new
    {
        acceptedDataPolicy = true,
        email = "someone@example.com",
        message = "Is the audit result cached anywhere?",
    };

    private static HttpRequestMessage ContactRequest(string clientAddress)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/contact")
        {
            Content = JsonContent.Create(ValidInquiry()),
        };

        // What Azure App Service puts in front of the container. Without the forwarded
        // headers middleware every caller would look like the load balancer and share a
        // single bucket.
        request.Headers.Add("X-Forwarded-For", clientAddress);

        return request;
    }

    [Test]
    public async Task AcceptsRequestsUpToTheLimit()
    {
        using var factory = new RateLimitedApiFactory(PermitLimit);
        using var client = factory.CreateClient();

        for (var attempt = 1; attempt <= PermitLimit; attempt++)
        {
            var response = await client.SendAsync(ContactRequest("203.0.113.10"));

            response
                .StatusCode
                .Should()
                .Be(HttpStatusCode.OK, $"request {attempt} is still within the limit of {PermitLimit}");
        }

        factory.SentMail.SendCount.Should().Be(PermitLimit);
    }

    [Test]
    public async Task RejectsTheRequestAfterTheLimitIsReached()
    {
        using var factory = new RateLimitedApiFactory(PermitLimit);
        using var client = factory.CreateClient();

        for (var attempt = 1; attempt <= PermitLimit; attempt++)
        {
            await client.SendAsync(ContactRequest("203.0.113.11"));
        }

        var rejected = await client.SendAsync(ContactRequest("203.0.113.11"));

        rejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Test]
    public async Task SendsNoFurtherMailOnceTheLimitIsReached()
    {
        // The status code alone would not prove the request was stopped before the
        // controller ran. The send count does.
        using var factory = new RateLimitedApiFactory(PermitLimit);
        using var client = factory.CreateClient();

        for (var attempt = 1; attempt <= PermitLimit + 5; attempt++)
        {
            await client.SendAsync(ContactRequest("203.0.113.12"));
        }

        factory.SentMail.SendCount.Should().Be(PermitLimit);
    }

    [Test]
    public async Task TellsTheClientWhenToComeBack()
    {
        using var factory = new RateLimitedApiFactory(PermitLimit);
        using var client = factory.CreateClient();

        for (var attempt = 1; attempt <= PermitLimit; attempt++)
        {
            await client.SendAsync(ContactRequest("203.0.113.13"));
        }

        var rejected = await client.SendAsync(ContactRequest("203.0.113.13"));

        rejected
            .Headers
            .RetryAfter
            .Should()
            .NotBeNull("a client that is not told when to retry will simply retry at once");
    }

    [Test]
    public async Task CountsEachClientSeparately()
    {
        // This is the case that fails silently without the forwarded headers middleware:
        // both callers would land in the same bucket and one would lock out the other.
        using var factory = new RateLimitedApiFactory(PermitLimit);
        using var client = factory.CreateClient();

        for (var attempt = 1; attempt <= PermitLimit; attempt++)
        {
            await client.SendAsync(ContactRequest("203.0.113.20"));
        }

        var exhausted = await client.SendAsync(ContactRequest("203.0.113.20"));
        var otherClient = await client.SendAsync(ContactRequest("203.0.113.21"));

        exhausted.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        otherClient.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task StillRejectsAnInvalidPayloadBeforeTheLimitApplies()
    {
        // Rate limiting must not become the only validation. A malformed request should
        // keep failing on its own merits.
        using var factory = new RateLimitedApiFactory(PermitLimit);
        using var client = factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/contact")
        {
            Content = JsonContent.Create(new { acceptedDataPolicy = false, email = "someone@example.com", message = "no consent" }),
        };
        request.Headers.Add("X-Forwarded-For", "203.0.113.30");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        factory.SentMail.SendCount.Should().Be(0);
    }

    // The cases below are the deployed topology: a browser talks to the frontend server,
    // and the frontend server calls this API. Every such request therefore arrives from
    // one and the same address, here FrontendServer, whoever the visitor is.

    private const string FrontendServer = "198.51.100.80";

    private const string ProxyKey = "test-proxy-key-0123456789-abcdefghij";

    private static HttpRequestMessage ContactRequestThroughTheFrontend(string visitorAddress, string? proxyKey)
    {
        var request = ContactRequest(FrontendServer);

        request.Headers.Add("X-AdoAuditor-Client-Address", visitorAddress);

        if (proxyKey is not null)
        {
            request.Headers.Add("X-AdoAuditor-Proxy-Key", proxyKey);
        }

        return request;
    }

    [Test]
    public async Task CountsVisitorsBehindTheFrontendSeparatelyWhenTheProxyKeyMatches()
    {
        using var factory = new RateLimitedApiFactory(PermitLimit, ProxyKey);
        using var client = factory.CreateClient();

        for (var attempt = 1; attempt <= PermitLimit; attempt++)
        {
            var accepted = await client.SendAsync(ContactRequestThroughTheFrontend("203.0.113.40", ProxyKey));

            accepted.StatusCode.Should().Be(HttpStatusCode.OK, $"request {attempt} of the first visitor is within the limit");
        }

        var exhausted = await client.SendAsync(ContactRequestThroughTheFrontend("203.0.113.40", ProxyKey));
        var otherVisitor = await client.SendAsync(ContactRequestThroughTheFrontend("203.0.113.41", ProxyKey));

        exhausted.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        otherVisitor
            .StatusCode
            .Should()
            .Be(HttpStatusCode.OK, "one visitor using up the budget must not lock out the next one");
    }

    [Test]
    public async Task SharesOneBudgetBehindTheFrontendWhenTheProxyKeyIsWrong()
    {
        // Anyone can send the address header. Believing it without the right key would
        // hand out a fresh budget for every address a caller cares to invent.
        using var factory = new RateLimitedApiFactory(PermitLimit, ProxyKey);
        using var client = factory.CreateClient();

        for (var attempt = 1; attempt <= PermitLimit; attempt++)
        {
            await client.SendAsync(ContactRequestThroughTheFrontend($"203.0.113.{50 + attempt}", "not-the-configured-proxy-key-0123456"));
        }

        var inventedAddress = await client.SendAsync(ContactRequestThroughTheFrontend("203.0.113.60", "not-the-configured-proxy-key-0123456"));

        inventedAddress.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Test]
    public async Task SharesOneBudgetBehindTheFrontendWhenTheProxyKeyIsMissing()
    {
        using var factory = new RateLimitedApiFactory(PermitLimit, ProxyKey);
        using var client = factory.CreateClient();

        for (var attempt = 1; attempt <= PermitLimit; attempt++)
        {
            await client.SendAsync(ContactRequestThroughTheFrontend($"203.0.113.{70 + attempt}", proxyKey: null));
        }

        var inventedAddress = await client.SendAsync(ContactRequestThroughTheFrontend("203.0.113.80", proxyKey: null));

        inventedAddress.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Test]
    public async Task IgnoresTheVisitorAddressWhenNoProxyKeyIsConfigured()
    {
        // A deployment that has not set the key yet must behave exactly as before, even
        // if a caller presents both headers.
        using var factory = new RateLimitedApiFactory(PermitLimit);
        using var client = factory.CreateClient();

        for (var attempt = 1; attempt <= PermitLimit; attempt++)
        {
            await client.SendAsync(ContactRequestThroughTheFrontend($"203.0.113.{90 + attempt}", ProxyKey));
        }

        var inventedAddress = await client.SendAsync(ContactRequestThroughTheFrontend("203.0.113.100", ProxyKey));

        inventedAddress.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Test]
    public async Task CountsADirectCallerUnderItsOwnAddress()
    {
        // The API is reachable without the frontend as well. Such a caller has no key and
        // is counted under its own address, separate from everyone the frontend vouches for.
        using var factory = new RateLimitedApiFactory(PermitLimit, ProxyKey);
        using var client = factory.CreateClient();

        for (var attempt = 1; attempt <= PermitLimit; attempt++)
        {
            await client.SendAsync(ContactRequestThroughTheFrontend("203.0.113.110", ProxyKey));
        }

        var direct = await client.SendAsync(ContactRequest("203.0.113.110"));

        direct.StatusCode.Should().Be(HttpStatusCode.TooManyRequests, "the same address is the same budget, with or without the frontend in between");

        var otherDirect = await client.SendAsync(ContactRequest("203.0.113.111"));

        otherDirect.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
