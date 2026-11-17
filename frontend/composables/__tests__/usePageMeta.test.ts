import { describe, expect, it, vi } from "vitest";
import { mockNuxtImport } from "@nuxt/test-utils/runtime";
import { usePageMeta } from "../usePageMeta";

const { useRouteMock } = vi.hoisted(() => ({
  useRouteMock: vi.fn(() => ({ meta: { title: "Audit" } })),
}));
mockNuxtImport("useRoute", () => useRouteMock);

describe("usePageMeta", () => {
  it("returns the meta of the current route", () => {
    expect(usePageMeta()).toEqual({ title: "Audit" });
  });
});
