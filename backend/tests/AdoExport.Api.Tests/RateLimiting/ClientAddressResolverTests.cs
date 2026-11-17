using System.Net;

using AdoExport.Api.RateLimiting;

using Microsoft.AspNetCore.Http;

namespace AdoExport.Api.Tests.RateLimiting;

public class ClientAddressResolverTests
{
    [Test]
    public void UsesAnIPv4AddressAsItIs()
    {
        ClientAddressResolver
            .Resolve(IPAddress.Parse("203.0.113.7"))
            .Should()
            .Be("203.0.113.7");
    }

    [Test]
    public void FallsBackToASharedKeyWhenTheAddressIsUnknown()
    {
        // A null address must not hand out an unlimited bucket per request. One shared
        // bucket is the safe reading of "we do not know who this is".
        ClientAddressResolver
            .Resolve(null)
            .Should()
            .Be(ClientAddressResolver.UnknownClient);
    }

    [Test]
    public void UnwrapsAnIPv4MappedAddressToTheSameKeyAsThePlainOne()
    {
        // A dual-stack socket reports 203.0.113.7 as ::ffff:203.0.113.7. Without the
        // unwrap the same caller would hold two separate budgets.
        var mapped = IPAddress.Parse("203.0.113.7").MapToIPv6();

        ClientAddressResolver
            .Resolve(mapped)
            .Should()
            .Be(ClientAddressResolver.Resolve(IPAddress.Parse("203.0.113.7")));
    }

    [Test]
    public void ReducesAnIPv6AddressToItsRoutingPrefix()
    {
        ClientAddressResolver
            .Resolve(IPAddress.Parse("2001:db8:1234:5678:9abc:def0:1234:5678"))
            .Should()
            .Be("2001:db8:1234:5678::/64");
    }

    [Test]
    public void GivesEveryAddressInOneIPv6SubnetTheSameKey()
    {
        // This is the point of the prefix: a subscriber holding a /64 can pick any
        // address inside it, so per-address limiting would be free to sidestep.
        var first = ClientAddressResolver.Resolve(IPAddress.Parse("2001:db8:1234:5678::1"));
        var second = ClientAddressResolver.Resolve(IPAddress.Parse("2001:db8:1234:5678:ffff:ffff:ffff:ffff"));

        first.Should().Be(second);
    }

    [Test]
    public void KeepsDifferentIPv6SubnetsApart()
    {
        var first = ClientAddressResolver.Resolve(IPAddress.Parse("2001:db8:1234:5678::1"));
        var second = ClientAddressResolver.Resolve(IPAddress.Parse("2001:db8:1234:5679::1"));

        first.Should().NotBe(second);
    }

    [Test]
    public void KeepsLoopbackAddressesApartFromRealClients()
    {
        ClientAddressResolver
            .Resolve(IPAddress.Loopback)
            .Should()
            .Be("127.0.0.1");
    }

    private const string ProxyKey = "test-proxy-key-0123456789-abcdefghij";

    private const string SocketAddress = "198.51.100.80";

    private static HttpContext RequestFromTheFrontend(string? visitorAddress, string? presentedKey)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(SocketAddress);

        if (visitorAddress is not null)
        {
            httpContext.Request.Headers[ClientAddressResolver.VisitorAddressHeader] = visitorAddress;
        }

        if (presentedKey is not null)
        {
            httpContext.Request.Headers[ClientAddressResolver.ProxyKeyHeader] = presentedKey;
        }

        return httpContext;
    }

    [Test]
    public void UsesTheVisitorAddressWhenTheProxyKeyMatches()
    {
        ClientAddressResolver
            .Resolve(RequestFromTheFrontend("203.0.113.7", ProxyKey), ProxyKey)
            .Should()
            .Be("203.0.113.7");
    }

    [Test]
    public void ReducesAnIPv6VisitorAddressToItsRoutingPrefix()
    {
        // The visitor address goes through the same reduction as a socket address, so the
        // /64 rule cannot be sidestepped by arriving through the frontend.
        ClientAddressResolver
            .Resolve(RequestFromTheFrontend("2001:db8:1234:5678:9abc:def0:1234:5678", ProxyKey), ProxyKey)
            .Should()
            .Be("2001:db8:1234:5678::/64");
    }

    [TestCase("not-the-configured-proxy-key-0123456", Description = "same length, different content")]
    [TestCase("short", Description = "different length")]
    [TestCase("", Description = "empty")]
    public void FallsBackToTheSocketAddressWhenTheProxyKeyIsWrong(string presentedKey)
    {
        ClientAddressResolver
            .Resolve(RequestFromTheFrontend("203.0.113.7", presentedKey), ProxyKey)
            .Should()
            .Be(SocketAddress);
    }

    [Test]
    public void FallsBackToTheSocketAddressWhenTheProxyKeyIsMissing()
    {
        ClientAddressResolver
            .Resolve(RequestFromTheFrontend("203.0.113.7", presentedKey: null), ProxyKey)
            .Should()
            .Be(SocketAddress);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void TrustsNobodyWhenNoProxyKeyIsConfigured(string? configuredKey)
    {
        // An empty configured key must never match an empty presented one. That would
        // turn "not set up yet" into "everyone is trusted".
        ClientAddressResolver
            .Resolve(RequestFromTheFrontend("203.0.113.7", configuredKey ?? string.Empty), configuredKey)
            .Should()
            .Be(SocketAddress);
    }

    [TestCase("not-an-address")]
    [TestCase("")]
    public void FallsBackToTheSocketAddressWhenTheVisitorAddressDoesNotParse(string visitorAddress)
    {
        ClientAddressResolver
            .Resolve(RequestFromTheFrontend(visitorAddress, ProxyKey), ProxyKey)
            .Should()
            .Be(SocketAddress);
    }

    [Test]
    public void FallsBackToTheSocketAddressWhenTheVisitorAddressIsMissing()
    {
        ClientAddressResolver
            .Resolve(RequestFromTheFrontend(visitorAddress: null, ProxyKey), ProxyKey)
            .Should()
            .Be(SocketAddress);
    }

    [Test]
    public void FallsBackToTheSocketAddressWhenAHeaderIsRepeated()
    {
        var httpContext = RequestFromTheFrontend("203.0.113.7", ProxyKey);
        httpContext.Request.Headers.Append(ClientAddressResolver.VisitorAddressHeader, "203.0.113.8");

        ClientAddressResolver
            .Resolve(httpContext, ProxyKey)
            .Should()
            .Be(SocketAddress);

        var repeatedKey = RequestFromTheFrontend("203.0.113.7", ProxyKey);
        repeatedKey.Request.Headers.Append(ClientAddressResolver.ProxyKeyHeader, ProxyKey);

        ClientAddressResolver
            .Resolve(repeatedKey, ProxyKey)
            .Should()
            .Be(SocketAddress);
    }
}
