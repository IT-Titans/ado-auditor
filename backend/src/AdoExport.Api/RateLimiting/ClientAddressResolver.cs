using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace AdoExport.Api.RateLimiting;

/// <summary>
/// Turns a caller's address into the key its rate limit is counted against.
/// </summary>
public static class ClientAddressResolver
{
    /// <summary>
    /// Used when the address cannot be determined, so those requests share one bucket
    /// instead of each getting an unlimited one of its own.
    /// </summary>
    public const string UnknownClient = "unknown-client";

    /// <summary>
    /// The header in which the frontend server passes on the address of the visitor it
    /// is acting for.
    /// </summary>
    public const string VisitorAddressHeader = "X-AdoAuditor-Client-Address";

    /// <summary>
    /// The header in which the frontend server proves that it is the one sending
    /// <see cref="VisitorAddressHeader" />.
    /// </summary>
    public const string ProxyKeyHeader = "X-AdoAuditor-Proxy-Key";

    /// <summary>
    /// The key for the caller behind <paramref name="httpContext" />.
    /// </summary>
    /// <remarks>
    /// Browsers never talk to this API directly. They talk to the frontend server, which
    /// calls the API on their behalf, so the socket address of almost every request is the
    /// frontend server and all visitors would share one budget. The frontend therefore
    /// passes the visitor's address along in <see cref="VisitorAddressHeader" />.
    /// <para>
    /// That header is only believed when the request also carries the shared secret in
    /// <see cref="ProxyKeyHeader" />. Anyone can send the address header, and a caller who
    /// could pick its own address would get a fresh budget with every request. Without
    /// the secret, with a wrong one, or with an address that does not parse, the socket
    /// address is used exactly as before.
    /// </para>
    /// </remarks>
    /// <param name="httpContext">The request to identify the caller of.</param>
    /// <param name="trustedProxyKey">
    /// The configured shared secret. When it is empty no caller is trusted and the
    /// visitor address header is ignored.
    /// </param>
    public static string Resolve(HttpContext httpContext, string? trustedProxyKey)
    {
        Guard.IsNotNull(httpContext);

        if (TryGetVisitorAddress(httpContext.Request.Headers, trustedProxyKey, out var visitorAddress))
        {
            return Resolve(visitorAddress);
        }

        return Resolve(httpContext.Connection.RemoteIpAddress);
    }

    /// <summary>
    /// The key for <paramref name="address" />.
    /// </summary>
    /// <remarks>
    /// IPv4 addresses are used as they are. IPv6 addresses are reduced to their /64
    /// prefix, because a single subscriber is routinely handed a whole /64 and can pick
    /// any address inside it at will - limiting one full IPv6 address would be sidestepped
    /// without even trying.
    /// <para>
    /// An IPv4-mapped IPv6 address is unwrapped first, so the same client gets the same
    /// key whether or not the socket is dual-stack.
    /// </para>
    /// </remarks>
    public static string Resolve(IPAddress? address)
    {
        if (address is null)
        {
            return UnknownClient;
        }

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return address.ToString();
        }

        var bytes = address.GetAddressBytes();

        // Keep the routing prefix, discard the interface identifier.
        Array.Clear(bytes, 8, 8);

        return new IPAddress(bytes) + "/64";
    }

    private static bool TryGetVisitorAddress(IHeaderDictionary headers, string? trustedProxyKey, out IPAddress? visitorAddress)
    {
        visitorAddress = null;

        if (string.IsNullOrWhiteSpace(trustedProxyKey))
        {
            return false;
        }

        // A repeated header is not something the frontend sends. Treating it as absent
        // is safer than guessing which of the values was meant.
        if (headers.TryGetValue(ProxyKeyHeader, out var presentedKeys) == false || presentedKeys.Count != 1)
        {
            return false;
        }

        // Compared in constant time, so the response time says nothing about how much of
        // a guessed key was right.
        var keyMatches = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(presentedKeys[0] ?? string.Empty),
            Encoding.UTF8.GetBytes(trustedProxyKey));
        if (keyMatches == false)
        {
            return false;
        }

        if (headers.TryGetValue(VisitorAddressHeader, out var presentedAddresses) == false || presentedAddresses.Count != 1)
        {
            return false;
        }

        return IPAddress.TryParse(presentedAddresses[0], out visitorAddress);
    }
}
