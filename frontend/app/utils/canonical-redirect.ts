export interface CanonicalRedirectInput {
  /**
   * Whether redirecting is switched on at all. Anything other than a strict `true`
   * counts as off, because an unset `NUXT_SHOULD_REDIRECT` must leave traffic alone.
   */
  shouldRedirect: unknown;

  /** The host every request should end up on, from `NUXT_REDIRECT_TO_URL`. */
  redirectToUrl: string | undefined;

  /** The `Host` header of the incoming request. */
  host: string | undefined;

  /**
   * The `x-forwarded-proto` header. Azure App Service terminates TLS at its front end,
   * so the scheme the client actually used is only visible here.
   */
  forwardedProto: string | undefined;

  /** The path and query to carry over to the redirect target. */
  fullPath: string;
}

/**
 * How the middleware hands the target to `navigateTo`. The target is always an absolute
 * URL, and `navigateTo` throws on one unless it is told the URL is external; the visitor
 * then gets a 500 instead of the redirect.
 */
export const canonicalRedirectOptions = {redirectCode: 301, external: true} as const;

/**
 * The absolute URL this request should be redirected to, or `null` when it should be
 * served as it is.
 *
 * Kept apart from the middleware on purpose: `import.meta.server` is replaced at
 * transform time, so a middleware body cannot be exercised in the Nuxt test environment.
 * Everything that decides anything lives here, and the middleware is left with wiring.
 */
export function resolveCanonicalRedirect(input: CanonicalRedirectInput): string | null {
  const {shouldRedirect, redirectToUrl, host, forwardedProto, fullPath} = input;

  if (shouldRedirect !== true) return null;

  // Without a target there is nothing to redirect to. A default host here would send
  // live traffic somewhere nobody configured.
  if (!redirectToUrl) return null;

  const proto = forwardedProto || 'http';

  if (proto === 'http') {
    // Upgrade the scheme but stay on the host the caller asked for; moving both at once
    // would turn one redirect into a guess about which host was meant.
    return `https://${host || redirectToUrl}${fullPath}`;
  }

  if (host !== redirectToUrl) {
    return `https://${redirectToUrl}${fullPath}`;
  }

  return null;
}
