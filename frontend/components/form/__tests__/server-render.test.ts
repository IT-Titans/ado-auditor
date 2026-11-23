import { afterEach, describe, expect, it, vi } from "vitest";
import FormStartAudit from "../FormStartAudit.vue";
import FormContact from "../FormContact.vue";
import FeedbackSendModal from "../../feedback/FeedbackSendModal.vue";
import { renderOnServer } from "./render-on-server";

// A form that exists in the server-rendered HTML can be submitted by the browser before the
// page has hydrated. Without a method the browser then sends every field as a GET query
// string, which puts the PAT into the address bar, the history and the server access log.
describe("server-rendered forms", () => {
  vi.spyOn(console, "warn").mockImplementation(() => {});

  // Rendering an icon on the server queues an icon load; let it run before the file ends.
  afterEach(() => new Promise((resolve) => setTimeout(resolve, 50)));

  const expectNoSubmittableForm = (html: string) => {
    expect(html).not.toMatch(/<form[\s>]/);
    expect(html).not.toMatch(/<input[\s>]/);
    expect(html).not.toMatch(/<textarea[\s>]/);
    expect(html).not.toMatch(/type="submit"/);
  };

  it("start audit form carries no form and no pat field", async () => {
    const html = await renderOnServer(FormStartAudit);
    expectNoSubmittableForm(html);
    expect(html).not.toContain('name="pat"');
    expect(html).not.toContain('type="password"');
  });

  it("contact form carries no form and no personal data field", async () => {
    expectNoSubmittableForm(await renderOnServer(FormContact));
  });

  it("feedback modal carries no form while closed", async () => {
    expectNoSubmittableForm(await renderOnServer(FeedbackSendModal));
  });
});
