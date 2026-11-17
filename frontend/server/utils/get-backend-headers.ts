import { type H3Event, getRequestHeader } from "h3";
import { resolveVisitorAddress } from "./visitor-address";

/** Carries the address of the visitor this server is calling the backend for. */
export const VISITOR_ADDRESS_HEADER = "x-adoauditor-client-address";

/** Carries the shared secret that makes the backend believe the address header. */
export const PROXY_KEY_HEADER = "x-adoauditor-proxy-key";

/**
 * The headers every call to the backend has to carry.
 *
 * The backend limits requests per caller, and its caller is always this server. Without
 * the visitor's address all visitors would be counted together and one of them could use
 * up the budget of everyone else. The backend only believes the address when it comes
 * with the shared secret from `NUXT_PROXY_KEY`, because the header alone could be sent by
 * anyone.
 *
 * Returns nothing when no secret is configured or the visitor cannot be identified. The
 * backend then counts by the address it sees, which is how it behaved before.
 *
 * @param event - The current H3Event from a server route
 */
export function getBackendHeaders(event: H3Event): Record<string, string> {
  const { proxyKey } = useRuntimeConfig();

  if (!proxyKey) {
    return {};
  }

  const visitorAddress = resolveVisitorAddress(
    getRequestHeader(event, "x-forwarded-for"),
    event.node.req.socket?.remoteAddress
  );

  if (!visitorAddress) {
    return {};
  }

  return {
    [VISITOR_ADDRESS_HEADER]: visitorAddress,
    [PROXY_KEY_HEADER]: proxyKey,
  };
}
