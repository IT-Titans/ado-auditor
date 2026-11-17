import { H3Error } from "h3";

export default defineEventHandler(async (event) => {
  const target = getProxiedUrl(event);

  try {
    const body = await readBody(event);

    await $fetch(target, {
      method: "POST",
      body,
      headers: getBackendHeaders(event),
      onResponseError: ({ response }) => {
        passRetryAfter(event, response);

        console.log("Response error:", response);

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
