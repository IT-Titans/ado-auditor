import { beforeEach, describe, expect, it, vi } from "vitest";
import type { H3Event } from "h3";
import {
  PROXY_KEY_HEADER,
  VISITOR_ADDRESS_HEADER,
  getBackendHeaders,
} from "../get-backend-headers";

const { useRuntimeConfigMock } = vi.hoisted(() => {
  return {
    useRuntimeConfigMock: vi.fn(() => {
      return { proxyKey: "shared-secret" };
    }),
  };
});

// Nitro provides useRuntimeConfig to server code as an auto-import. Without the server
// build the function under test looks it up as a global, which is where the mock goes.
vi.stubGlobal("useRuntimeConfig", useRuntimeConfigMock);

function eventWith(
  headers: Record<string, string>,
  remoteAddress?: string
): H3Event {
  return {
    node: { req: { headers, socket: { remoteAddress } } },
  } as unknown as H3Event;
}

describe("getBackendHeaders", () => {
  beforeEach(() => {
    useRuntimeConfigMock.mockImplementation(() => {
      return { proxyKey: "shared-secret" };
    });
  });

  it("sends the visitor address together with the shared secret", () => {
    const event = eventWith({ "x-forwarded-for": "1.1.1.1, 198.51.100.7:51234" });

    expect(getBackendHeaders(event)).toEqual({
      [VISITOR_ADDRESS_HEADER]: "198.51.100.7",
      [PROXY_KEY_HEADER]: "shared-secret",
    });
  });

  it("uses the socket address when nothing was forwarded", () => {
    const event = eventWith({}, "127.0.0.1");

    expect(getBackendHeaders(event)).toEqual({
      [VISITOR_ADDRESS_HEADER]: "127.0.0.1",
      [PROXY_KEY_HEADER]: "shared-secret",
    });
  });

  it("sends nothing when no secret is configured", () => {
    // Without the secret the backend would not believe the address anyway, and sending
    // an address alone would only suggest that it does.
    useRuntimeConfigMock.mockImplementation(() => {
      return { proxyKey: "" };
    });
    const event = eventWith({ "x-forwarded-for": "198.51.100.7:51234" });

    expect(getBackendHeaders(event)).toEqual({});
  });

  it("sends nothing when the visitor cannot be identified", () => {
    // The secret must not travel without an address: it would vouch for nothing.
    const event = eventWith({ "x-forwarded-for": "unknown" });

    expect(getBackendHeaders(event)).toEqual({});
  });

  it("copes with a request that has no socket", () => {
    const event = {
      node: { req: { headers: {} } },
    } as unknown as H3Event;

    expect(getBackendHeaders(event)).toEqual({});
  });
});
