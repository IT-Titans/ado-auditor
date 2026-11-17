import { describe, expect, it, vi } from "vitest";
import type { H3Event } from "h3";
import { passRetryAfter } from "../pass-retry-after";

function eventRecordingHeaders() {
  const setHeader = vi.fn();
  const event = { node: { res: { setHeader } } } as unknown as H3Event;

  return { event, setHeader };
}

function response(status: number, headers: Record<string, string> = {}) {
  return { status, headers: new Headers(headers) };
}

describe("passRetryAfter", () => {
  it("copies Retry-After from a 429 onto the response", () => {
    const { event, setHeader } = eventRecordingHeaders();

    passRetryAfter(event, response(429, { "Retry-After": "600" }));

    expect(setHeader).toHaveBeenCalledWith("Retry-After", "600");
  });

  it("leaves the response alone when the 429 carries no Retry-After", () => {
    const { event, setHeader } = eventRecordingHeaders();

    passRetryAfter(event, response(429));

    expect(setHeader).not.toHaveBeenCalled();
  });

  it("does not pass the header on for any other status", () => {
    // A Retry-After on a 503 from the backend is about the backend, not about how often
    // this visitor may ask.
    const { event, setHeader } = eventRecordingHeaders();

    passRetryAfter(event, response(503, { "Retry-After": "30" }));

    expect(setHeader).not.toHaveBeenCalled();
  });

  it("copes with a response that has no headers", () => {
    const { event, setHeader } = eventRecordingHeaders();

    passRetryAfter(event, { status: 429 });

    expect(setHeader).not.toHaveBeenCalled();
  });

  it("copes with a missing response", () => {
    const { event, setHeader } = eventRecordingHeaders();

    passRetryAfter(event, undefined);

    expect(setHeader).not.toHaveBeenCalled();
  });
});
