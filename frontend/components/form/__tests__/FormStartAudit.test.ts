import { beforeEach, describe, expect, it, vi } from "vitest";
import { mockNuxtImport } from "@nuxt/test-utils/runtime";
import FormStartAudit from "../FormStartAudit.vue";
import { mountInApp } from "./mount-in-app";

const { navigateToMock, fetchMock } = vi.hoisted(() => ({
  navigateToMock: vi.fn(),
  fetchMock: vi.fn(),
}));

mockNuxtImport("navigateTo", () => navigateToMock);

const mountForm = async () => {
  const wrapper = await mountInApp(FormStartAudit);
  await vi.waitFor(() => expect(wrapper.find("form").exists()).toBe(true));
  return wrapper;
};

describe("FormStartAudit after hydration", () => {
  beforeEach(() => {
    fetchMock.mockReset();
    navigateToMock.mockReset();
    vi.stubGlobal("$fetch", fetchMock);
    vi.spyOn(console, "error").mockImplementation(() => {});
  });

  it("shows the form with a masked pat field", async () => {
    const wrapper = await mountForm();
    expect(wrapper.find('input[type="password"]').exists()).toBe(true);
    expect(wrapper.text()).toContain("Organization Name");
  });

  it("does not submit an empty form and names the missing fields", async () => {
    const wrapper = await mountForm();
    await wrapper.find("form").trigger("submit");
    await vi.waitFor(() =>
      expect(wrapper.text()).toContain("Personal Access Token is required.")
    );
    expect(wrapper.text()).toContain("Organization Name is required.");
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("posts a valid form as json and opens the audit", async () => {
    fetchMock.mockResolvedValue({ data: { id: "abc" } });
    const wrapper = await mountForm();
    const [org, pat, email] = wrapper.findAll("input:not([type=checkbox]):not([data-hidden])");
    await org.setValue("contoso");
    await pat.setValue("secret-token");
    await email.setValue("a@b.de");
    await wrapper.find('button[role="checkbox"]').trigger("click");
    await wrapper.find("form").trigger("submit");

    await vi.waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1));
    const [url, options] = fetchMock.mock.calls[0];
    expect(url).toBe("/api/audit");
    expect(options.method).toBe("POST");
    expect(JSON.parse(options.body)).toMatchObject({
      organization: "contoso",
      pat: "secret-token",
      email: "a@b.de",
      acceptedDataPolicy: true,
    });
    await vi.waitFor(() => expect(navigateToMock).toHaveBeenCalledWith("/audit/abc"));
  });

  it("shows the invalid credentials message for a 401", async () => {
    fetchMock.mockImplementation(async (_url, options) => {
      options.onResponseError({ response: { status: 401 } });
    });
    const wrapper = await mountForm();
    const [org, pat, email] = wrapper.findAll("input:not([type=checkbox]):not([data-hidden])");
    await org.setValue("contoso");
    await pat.setValue("bad");
    await email.setValue("a@b.de");
    await wrapper.find('button[role="checkbox"]').trigger("click");
    await wrapper.find("form").trigger("submit");

    await vi.waitFor(() => expect(wrapper.text()).toContain("Invalid Organization Name or PAT."));
  });
});
