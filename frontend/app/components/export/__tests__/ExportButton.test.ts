import { beforeEach, describe, expect, it, vi } from "vitest";
import { mockNuxtImport, mountSuspended } from "@nuxt/test-utils/runtime";
import { ref } from "vue";
import ExportButton from "../ExportButton.vue";

const { useAsyncDataMock } = vi.hoisted(() => ({ useAsyncDataMock: vi.fn() }));

// Nuxt calls useAsyncData itself while the application starts, which happens after this
// mock is registered. It hands over to the real one until a test replaces it.
mockNuxtImport("useAsyncData", (original) =>
  useAsyncDataMock.mockImplementation(original as typeof useAsyncData)
);

const clickExport = async () => {
  const wrapper = await mountSuspended(ExportButton);
  const vm = wrapper.vm as unknown as { handleClick: () => Promise<void> };
  return vm.handleClick();
};

describe("ExportButton", () => {
  const createObjectURL = vi.fn(() => "blob:report");

  beforeEach(() => {
    useAsyncDataMock.mockReset();
    createObjectURL.mockClear();
    vi.stubGlobal("URL", Object.assign(URL, { createObjectURL, revokeObjectURL: vi.fn() }));
    vi.spyOn(console, "error").mockImplementation(() => {});
    vi.spyOn(console, "log").mockImplementation(() => {});
  });

  it("offers the downloaded report as a blob url", async () => {
    const blob = new Blob(["pdf"]);
    useAsyncDataMock.mockResolvedValue({ data: ref(blob), error: ref(null) });

    await clickExport();

    expect(createObjectURL).toHaveBeenCalledWith(blob);
  });

  it("does not build a download link when the report is empty", async () => {
    useAsyncDataMock.mockResolvedValue({ data: ref(null), error: ref(null) });

    await expect(clickExport()).rejects.toBeDefined();

    expect(createObjectURL).not.toHaveBeenCalled();
  });

  it("does not build a download link when the request failed", async () => {
    useAsyncDataMock.mockResolvedValue({ data: ref(null), error: ref(new Error("boom")) });

    await expect(clickExport()).rejects.toBeDefined();

    expect(createObjectURL).not.toHaveBeenCalled();
  });
});
