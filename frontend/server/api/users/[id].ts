import { H3Error } from "h3";
import type { UserDetailsResponse } from "~/types/users";

export default defineEventHandler(
  async (event): Promise<UserDetailsResponse> => {
    const target = getProxiedUrl(event);

    try {
      const query = getQuery(event);
      return await $fetch<UserDetailsResponse>(target, {
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
  }
);
