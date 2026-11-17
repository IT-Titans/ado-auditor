export default defineNuxtRouteMiddleware((to) => {
  if (!import.meta.server) return;

  const { shouldRedirect, redirectToUrl } = useRuntimeConfig();

  // useRequestHeaders can come back undefined when there is no incoming request to read
  // from, so this cannot be destructured straight away.
  const headers = useRequestHeaders(['host', 'x-forwarded-proto']) ?? {};

  // All decisions live in resolveCanonicalRedirect, which is unit tested; this stays wiring.
  const target = resolveCanonicalRedirect({
    shouldRedirect,
    redirectToUrl: redirectToUrl as string | undefined,
    host: headers.host,
    forwardedProto: headers['x-forwarded-proto'],
    fullPath: to.fullPath,
  });

  if (target) {
    return navigateTo(target, { redirectCode: 301 });
  }
})
