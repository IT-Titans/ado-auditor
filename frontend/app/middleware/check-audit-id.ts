import * as z from 'zod';

const auditIdSchema = z.guid();

export default defineNuxtRouteMiddleware((to ) => {
  if (!isEmptyObject(to.params)) {
    const id = to.params.id;

    const auditId = z.safeParse(auditIdSchema, id);

    if (!auditId.success) {
      return navigateTo('/audit');
    }
  }
});