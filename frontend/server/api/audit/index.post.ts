export default defineEventHandler(async (event) => {
  const target = getProxiedUrl(event);

  const body = await readBody(event);

  const result = await $fetch<string>(target, {
    method: "POST",
     body,
  });

  return {
    status: 200,
    data: {
      organization: body.organization,
      id: result,
    },
  };
});
