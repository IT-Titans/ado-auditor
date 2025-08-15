export default defineEventHandler(async (event) => {
  const target = getProxiedUrl(event);
  try {
    return await proxyRequest(event, target);
  } catch (e) {
    console.error("Proxy error: ", e);
    throw createError({ status: 500, message: "Proxy error", data: e });
  }
});
