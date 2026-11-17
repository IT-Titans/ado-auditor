export default defineEventHandler(async (event) => {
  const target = getProxiedUrl(event);

  const body = await readBody(event);

  const result = await $fetch<string>(target, {
    method: "POST",
    body,
    headers: getBackendHeaders(event),
    // Nothing is thrown here on purpose: the error keeps reaching the caller exactly as
    // before, it only carries the backend's Retry-After along when there is one.
    onResponseError: ({ response }) => {
      passRetryAfter(event, response);
    },
  });

  return {
    status: 200,
    data: {
      organization: body.organization,
      id: result,
    },
  };
});
