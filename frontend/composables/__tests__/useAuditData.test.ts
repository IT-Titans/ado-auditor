import { beforeEach, describe, expect, it, vi } from "vitest";
import { mockNuxtImport } from "@nuxt/test-utils/runtime";
import { ref } from "vue";
import { useAuditData } from "../useAuditData";

const { replaceMock, paramsMock, useStateMock, useAsyncDataMock } = vi.hoisted(() => ({
  replaceMock: vi.fn(),
  useStateMock: vi.fn(),
  useAsyncDataMock: vi.fn(),
  paramsMock: { id: "audit-1" } as Record<string, string>,
}));

mockNuxtImport("useRouter", () => () => ({
  currentRoute: { value: { params: paramsMock } },
  replace: replaceMock,
}));
mockNuxtImport("useState", () => useStateMock);
mockNuxtImport("useAsyncData", () => useAsyncDataMock);

const baseData = () => ({
  summaries: [
    { category: "a" },
    { category: "b" },
    { category: "placeholder" },
    { category: "placeholder" },
  ],
});

describe("useAuditData", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.spyOn(console, "error").mockImplementation(() => {});
  });

  it("returns the cached state without fetching", async () => {
    const state = ref({ summaries: [] });
    useStateMock.mockReturnValue(state);

    const result = await useAuditData();

    expect(result).toBe(state);
    expect(useAsyncDataMock).not.toHaveBeenCalled();
  });

  it("keys the state and the fetch by the audit id", async () => {
    useStateMock.mockReturnValue(ref(null));
    useAsyncDataMock.mockResolvedValue({
      data: ref(baseData()),
      error: ref(null),
    });

    await useAuditData();

    expect(useStateMock.mock.calls[0][0]).toBe("audit-data-audit-1");
    expect(useAsyncDataMock.mock.calls[0][0]).toBe("audit-data-audit-1");
  });

  it("fetches the audit from the API with a ten minute timeout", async () => {
    useStateMock.mockReturnValue(ref(null));
    useAsyncDataMock.mockResolvedValue({
      data: ref(baseData()),
      error: ref(null),
    });
    const fetchSpy = vi.fn().mockResolvedValue({});
    vi.stubGlobal("$fetch", fetchSpy);

    await useAuditData();
    const fetcher = useAsyncDataMock.mock.calls[0][1] as () => unknown;
    await fetcher();

    expect(fetchSpy).toHaveBeenCalledWith("/api/audit/audit-1", {
      timeout: 1000 * 60 * 10,
    });
    vi.unstubAllGlobals();
  });

  it("adds the placeholder summaries for projects and groups and stores the result", async () => {
    const state = ref<unknown>(null);
    const data = ref(baseData());
    useStateMock.mockReturnValue(state);
    useAsyncDataMock.mockResolvedValue({ data, error: ref(null) });

    const result = await useAuditData();

    const summaries = (result.value as unknown as ReturnType<typeof baseData>)
      .summaries as unknown as Array<{
      scope: string;
      items: Array<{ severity: string }>;
    }>;
    expect(summaries[2].scope).toBe("projects");
    expect(summaries[3].scope).toBe("groups");
    expect(summaries[2].items.map((i) => i.severity)).toEqual([
      "critical",
      "warning",
      "ok",
    ]);
    expect(summaries[3].items).toHaveLength(3);
    expect(state.value).toBe(data.value);
  });

  it.each([401, 410])(
    "redirects to the expired page when the API answers %i",
    async (statusCode) => {
      useStateMock.mockReturnValue(ref(null));
      useAsyncDataMock.mockResolvedValue({
        data: ref(null),
        error: ref({ statusCode }),
      });

      // With no data the composable still dereferences it afterwards and throws; the
      // redirect has already been issued by then, which is what is asserted here.
      await useAuditData().catch(() => {});

      expect(replaceMock).toHaveBeenCalledWith("/audit?expired=true");
    }
  );

  it("does not redirect for other errors", async () => {
    useStateMock.mockReturnValue(ref(null));
    useAsyncDataMock.mockResolvedValue({
      data: ref(null),
      error: ref({ statusCode: 500 }),
    });

    await useAuditData().catch(() => {});

    expect(replaceMock).not.toHaveBeenCalled();
  });
});
