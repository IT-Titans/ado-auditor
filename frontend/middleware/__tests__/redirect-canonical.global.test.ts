import { describe, expect, it } from "vitest";
import type { RouteLocationNormalized } from "vue-router";
import middleware from "../01.redirect-canonical.global";

// import.meta.server reads false in the Nuxt test environment and cannot be assigned, so
// only the client-side guard of the middleware is reachable here. The redirect decisions
// are covered in utils/__tests__/canonical-redirect.test.ts.
describe("01.redirect-canonical.global middleware", () => {
  it("does nothing outside the server", () => {
    const result = (middleware as (to: RouteLocationNormalized) => unknown)({
      fullPath: "/audit",
    } as RouteLocationNormalized);
    expect(result).toBeUndefined();
  });
});
