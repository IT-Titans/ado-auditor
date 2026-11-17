import type { H3Event } from "h3";

interface UpstreamResponse {
  status: number;
  headers?: { get(name: string): string | null };
}

/**
 * Hands the backend's `Retry-After` on to the caller when the backend rejected the
 * request with 429.
 *
 * The error this server answers with is built from the status and the body only, so the
 * header would be lost on the way and the browser would not learn when a retry can
 * succeed. Setting it on the response before the error is thrown keeps it.
 *
 * @param event - The current H3Event from a server route
 * @param response - The response the backend gave
 */
export function passRetryAfter(
  event: H3Event,
  response: UpstreamResponse | undefined
): void {
  if (response?.status !== 429) {
    return;
  }

  const retryAfter = response.headers?.get("retry-after");

  if (retryAfter) {
    // Written through Node's own API so the value goes out exactly as the backend sent
    // it. The header may hold seconds or a date, and h3's typed helper takes seconds only.
    event.node.res.setHeader("Retry-After", retryAfter);
  }
}
