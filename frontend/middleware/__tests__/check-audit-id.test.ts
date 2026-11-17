import { beforeEach, describe, expect, it, vi } from "vitest";
import { mockNuxtImport } from "@nuxt/test-utils/runtime";
import type { RouteLocationNormalized } from "vue-router";
import middleware from "../check-audit-id";

const { navigateToMock } = vi.hoisted(() => ({
  navigateToMock: vi.fn((to: string) => to),
}));

mockNuxtImport("navigateTo", () => navigateToMock);

const run = (params: Record<string, string>) =>
  (middleware as (to: RouteLocationNormalized) => unknown)({
    params,
  } as unknown as RouteLocationNormalized);

describe("check-audit-id middleware", () => {
  beforeEach(() => navigateToMock.mockClear());

  it("lets a valid GUID through", () => {
    run({ id: "123e4567-e89b-42d3-a456-426614174000" });
    expect(navigateToMock).not.toHaveBeenCalled();
  });

  it("redirects an id that is not a GUID to the audit start page", () => {
    expect(run({ id: "not-a-guid" })).toBe("/audit");
    expect(navigateToMock).toHaveBeenCalledWith("/audit");
  });

  it("redirects when the route has params but no id", () => {
    run({ other: "x" });
    expect(navigateToMock).toHaveBeenCalledWith("/audit");
  });

  it("ignores routes without params", () => {
    run({});
    expect(navigateToMock).not.toHaveBeenCalled();
  });
});
