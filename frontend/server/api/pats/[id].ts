import { H3Error } from "h3";
import type { PatDetailResponse } from "~/types/pats";

export default defineEventHandler(async (event): Promise<PatDetailResponse> => {
  const target = getProxiedUrl(event);

  try {
    const query = getQuery(event);
    return await $fetch<PatDetailResponse>(target, {
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
