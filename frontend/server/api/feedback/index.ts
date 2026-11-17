import { H3Error } from "h3";

export default defineEventHandler(async (event) => {
  const target = getProxiedUrl(event);

  try {
    const body = await readBody(event);
    const query = getQuery(event);

    await $fetch(target, {
      method: "POST",
      body,
      query: {
        auditId: query.auditId,
      },
      headers: getBackendHeaders(event),
      onResponseError: ({ response }) => {
        passRetryAfter(event, response);

        console.error("Response error:", response);

        throw createError({
          statusCode: response.status,
          statusMessage: response.statusText,
          data: response._data,
        });
      },
    });
  } catch (e) {
    if (e instanceof H3Error) {
      throw e;
    }
    throw createError({
      status: 500,
      message: "Internal Server Error",
      data: e,
    });
  }
});
