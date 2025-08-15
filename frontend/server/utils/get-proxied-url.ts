import { joinURL } from "ufo";
import { type H3Event, createError } from "h3";

/**
 * Constructs a full proxy target URL using the apiUrl from runtime config.
 * Throws an error if apiUrl is not configured.
 *
 * @param event - The current H3Event from a server route or middleware
 * @returns - The full proxied URL
 */
export function getProxiedUrl(event: H3Event): string {
  const config = useRuntimeConfig();
  const proxyUrl = config.apiUrl;

  if (!proxyUrl) {
    throw createError({
      statusCode: 500,
      statusMessage: "API URL is not configured in runtime config",
    });
  }

  return joinURL(proxyUrl, event.path);
}
