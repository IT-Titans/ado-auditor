import { mockNuxtImport } from "@nuxt/test-utils/runtime";
import { beforeEach, describe, expect, it, vi } from "vitest";
import FormContact from "../FormContact.vue";
import { mountInApp } from "./mount-in-app";

const { fetchMock, toastAddMock } = vi.hoisted(() => ({ fetchMock: vi.fn(), toastAddMock: vi.fn() }));

// A real toast starts a 5 s timer inside the toast provider that outlives the test file.
mockNuxtImport("useToast", () => () => ({ add: toastAddMock }));

const mountForm = async () => {
  const wrapper = await mountInApp(FormContact);
  await vi.waitFor(() => expect(wrapper.find("form").exists()).toBe(true));
  return wrapper;
};

describe("FormContact after hydration", () => {
  beforeEach(() => {
    fetchMock.mockReset();
    toastAddMock.mockReset();
    vi.stubGlobal("$fetch", fetchMock);
  });

  it("does not submit without a message and consent", async () => {
    const wrapper = await mountForm();
    await wrapper.find("form").trigger("submit");
    await vi.waitFor(() => expect(wrapper.text()).toContain("Please provide your message"));
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("posts a valid message as json", async () => {
    fetchMock.mockResolvedValue({});
    const wrapper = await mountForm();
    await wrapper.find("textarea").setValue("Hello");
    const inputs = wrapper.findAll("input:not([type=checkbox]):not([data-hidden])");
    await inputs[inputs.length - 1].setValue("a@b.de");
    await wrapper.find('button[role="checkbox"]').trigger("click");
    await wrapper.find("form").trigger("submit");

    await vi.waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1));
    const [url, options] = fetchMock.mock.calls[0];
    expect(url).toBe("/api/contact");
    expect(options.method).toBe("POST");
    expect(JSON.parse(options.body)).toMatchObject({ message: "Hello", email: "a@b.de", acceptedDataPolicy: true });
    await vi.waitFor(() => expect(toastAddMock).toHaveBeenCalledWith(expect.objectContaining({ title: "Success", color: "success" })));
  });
});
