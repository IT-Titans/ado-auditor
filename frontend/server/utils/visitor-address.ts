import { isIP } from "node:net";

/**
 * The address of the visitor a request to this server came from, or `undefined` when it
 * cannot be told.
 *
 * Azure App Service terminates the connection at its front end and appends the caller's
 * address to `X-Forwarded-For`. The entry it appended is the last one. Everything before
 * it was sent by the caller itself and can say anything, so the first entry, which is what
 * h3's `getRequestIP` returns, must not be used: a visitor could pick the address its
 * rate limit is counted under.
 *
 * This holds as long as exactly one proxy stands in front of this server. With a second
 * one in front (a CDN, for example) the last entry would be that proxy.
 *
 * @param forwardedFor - The `X-Forwarded-For` header of the incoming request
 * @param socketAddress - The remote address of the socket, used when the header is absent
 *   or its last entry is not an address, as it is in local development
 */
export function resolveVisitorAddress(
  forwardedFor: string | undefined,
  socketAddress: string | undefined
): string | undefined {
  const lastHop = forwardedFor?.split(",").pop();

  return toAddress(lastHop) ?? toAddress(socketAddress);
}

function toAddress(candidate: string | undefined): string | undefined {
  if (!candidate) {
    return undefined;
  }

  const address = stripPort(candidate.trim());

  return isIP(address) === 0 ? undefined : address;
}

/**
 * App Service writes the entry as `203.0.113.7:51234`, and an IPv6 one as
 * `[2001:db8::1]:51234`. A bare IPv6 address has several colons and no brackets, so it
 * is left alone.
 */
function stripPort(value: string): string {
  const bracketed = /^\[([^\]]+)\](?::\d+)?$/.exec(value);
  if (bracketed) {
    return bracketed[1]!;
  }

  const withPort = /^([^:]+):\d+$/.exec(value);
  if (withPort) {
    return withPort[1]!;
  }

  return value;
}
