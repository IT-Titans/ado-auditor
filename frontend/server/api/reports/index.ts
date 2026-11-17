export default defineEventHandler(async (event) => {
  const target = getProxiedUrl(event);

  try {
    return await proxyRequest(event, target, { headers: getBackendHeaders(event) });
  } catch (e) {
    console.error("Error in proxy request:", e);
    throw createError({ status: 401, message: "Unauthorized", data: e });
  }
});
