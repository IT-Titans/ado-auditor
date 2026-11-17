import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { createError, H3Error } from "h3";

type Handler = (event: unknown) => Promise<unknown>;
type FetchOptions = {
  method?: string;
  body?: unknown;
  query?: Record<string, unknown>;
  headers?: Record<string, string>;
  onResponseError?: (ctx: {
    response: { status: number; statusText: string; _data: unknown };
  }) => void;
};

const TARGET = "https://api.example.com/proxied";
const event = { path: "/api/x" };

// What a route has to hand to the backend so the visitor is counted on its own there.
const BACKEND_HEADERS = {
  "x-adoauditor-client-address": "203.0.113.7",
  "x-adoauditor-proxy-key": "key",
};

const getProxiedUrl = vi.fn((_event: unknown) => TARGET);
const proxyRequest = vi.fn();
const fetchMock = vi.fn();
const readBody = vi.fn();
const getQuery = vi.fn();
const getBackendHeaders = vi.fn((_event: unknown) => BACKEND_HEADERS);
const passRetryAfter = vi.fn();

async function load(path: string): Promise<Handler> {
  vi.resetModules();
  const mod = await import(/* @vite-ignore */ path);
  return mod.default as Handler;
}

beforeEach(() => {
  vi.stubGlobal("defineEventHandler", (fn: Handler) => fn);
  vi.stubGlobal("createError", createError);
  vi.stubGlobal("getProxiedUrl", getProxiedUrl);
  vi.stubGlobal("proxyRequest", proxyRequest);
  vi.stubGlobal("$fetch", fetchMock);
  vi.stubGlobal("readBody", readBody);
  vi.stubGlobal("getQuery", getQuery);
  vi.stubGlobal("getBackendHeaders", getBackendHeaders);
  vi.stubGlobal("passRetryAfter", passRetryAfter);
  vi.spyOn(console, "error").mockImplementation(() => {});
  vi.spyOn(console, "log").mockImplementation(() => {});
  getProxiedUrl.mockClear();
  proxyRequest.mockReset();
  fetchMock.mockReset();
  readBody.mockReset();
  getQuery.mockReset();
  getBackendHeaders.mockClear();
  passRetryAfter.mockReset();
  getQuery.mockReturnValue({ auditId: "audit-1" });
});

afterEach(() => {
  vi.unstubAllGlobals();
  vi.restoreAllMocks();
});

describe.each([
  ["audit/[id]", "../audit/[id]", 500, "Proxy error"],
  ["rules", "../rules/index", 500, "Proxy error"],
  ["rules/[scope]", "../rules/[scope]/index", 500, "Proxy error"],
  ["rules/[scope]/[ruleId]", "../rules/[scope]/[ruleId]", 500, "Proxy error"],
  ["reports", "../reports/index", 401, "Unauthorized"],
])("proxy handler %s", (_name, path, failStatus, failMessage) => {
  it("forwards the request to the proxied target and returns its result", async () => {
    proxyRequest.mockResolvedValue({ ok: true });
    const handler = await load(path);

    await expect(handler(event)).resolves.toEqual({ ok: true });
    expect(proxyRequest).toHaveBeenCalledWith(event, TARGET, {
      headers: BACKEND_HEADERS,
    });
    expect(getBackendHeaders).toHaveBeenCalledWith(event);
  });

  it(`maps a proxy failure to ${failStatus}`, async () => {
    const cause = new Error("boom");
    proxyRequest.mockRejectedValue(cause);
    const handler = await load(path);

    const error = await handler(event).catch((e: unknown) => e);
    expect(error).toBeInstanceOf(H3Error);
    expect((error as H3Error).statusCode).toBe(failStatus);
    expect((error as H3Error).message).toBe(failMessage);
    expect((error as H3Error).data).toBe(cause);
  });
});

describe.each([
  ["pats", "../pats/index"],
  ["pats/[id]", "../pats/[id]"],
  ["users", "../users/index"],
  ["users/[id]", "../users/[id]"],
  ["feedback", "../feedback/index"],
  ["contact", "../contact/index"],
])("upstream handler %s", (name, path) => {
  const isPost = name === "feedback" || name === "contact";

  it("calls the upstream target and passes the audit id as query", async () => {
    fetchMock.mockResolvedValue({ items: [] });
    readBody.mockResolvedValue({ message: "hi" });
    const handler = await load(path);

    await handler(event);

    const [target, options] = fetchMock.mock.calls[0] as [string, FetchOptions];
    expect(target).toBe(TARGET);
    expect(options.headers).toEqual(BACKEND_HEADERS);
    expect(getBackendHeaders).toHaveBeenCalledWith(event);
    if (name !== "contact") {
      expect(options.query).toEqual({ auditId: "audit-1" });
    }
    if (isPost) {
      expect(options.method).toBe("POST");
      expect(options.body).toEqual({ message: "hi" });
    }
  });

  it("returns the upstream result for read endpoints", async () => {
    if (isPost) return;
    fetchMock.mockResolvedValue({ items: [1] });
    const handler = await load(path);

    await expect(handler(event)).resolves.toEqual({ items: [1] });
  });

  it("rethrows an upstream error response with its status and data", async () => {
    fetchMock.mockImplementation(async (_t: string, options: FetchOptions) => {
      options.onResponseError?.({
        response: { status: 410, statusText: "Gone", _data: { reason: "x" } },
      });
    });
    readBody.mockResolvedValue({});
    const handler = await load(path);

    const error = await handler(event).catch((e: unknown) => e);
    expect(error).toBeInstanceOf(H3Error);
    expect((error as H3Error).statusCode).toBe(410);
    expect((error as H3Error).statusMessage).toBe("Gone");
    expect((error as H3Error).data).toEqual({ reason: "x" });
  });

  it("hands a rejected response to the Retry-After pass-through before failing", async () => {
    const response = { status: 429, statusText: "Too Many Requests", _data: null };
    fetchMock.mockImplementation(async (_t: string, options: FetchOptions) => {
      options.onResponseError?.({ response });
    });
    readBody.mockResolvedValue({});
    const handler = await load(path);

    const error = await handler(event).catch((e: unknown) => e);

    expect(passRetryAfter).toHaveBeenCalledWith(event, response);
    expect((error as H3Error).statusCode).toBe(429);
  });

  it("wraps an unexpected failure as 500", async () => {
    const cause = new Error("network down");
    fetchMock.mockRejectedValue(cause);
    readBody.mockResolvedValue({});
    const handler = await load(path);

    const error = await handler(event).catch((e: unknown) => e);
    expect(error).toBeInstanceOf(H3Error);
    expect((error as H3Error).statusCode).toBe(500);
    expect((error as H3Error).message).toBe("Internal Server Error");
    expect((error as H3Error).data).toBe(cause);
  });
});

describe("audit create handler", () => {
  it("posts the body upstream and returns organization and created id", async () => {
    readBody.mockResolvedValue({ organization: "contoso", pat: "secret" });
    fetchMock.mockResolvedValue("audit-42");
    const handler = await load("../audit/index.post");

    const result = await handler(event);

    const [target, options] = fetchMock.mock.calls[0] as [string, FetchOptions];
    expect(target).toBe(TARGET);
    expect(options.method).toBe("POST");
    expect(options.body).toEqual({ organization: "contoso", pat: "secret" });
    expect(options.headers).toEqual(BACKEND_HEADERS);
    expect(result).toEqual({
      status: 200,
      data: { organization: "contoso", id: "audit-42" },
    });
  });

  it("passes Retry-After on without replacing the upstream error", async () => {
    // Starting an audit is the tightest limit a visitor can run into. The error itself
    // has to stay what it was, because the form reads its status.
    const upstream = Object.assign(new Error("429 Too Many Requests"), { statusCode: 429 });
    const response = { status: 429, statusText: "Too Many Requests", _data: null };
    readBody.mockResolvedValue({ organization: "contoso", pat: "secret" });
    fetchMock.mockImplementation(async (_t: string, options: FetchOptions) => {
      expect(() => options.onResponseError?.({ response })).not.toThrow();
      throw upstream;
    });
    const handler = await load("../audit/index.post");

    await expect(handler(event)).rejects.toBe(upstream);
    expect(passRetryAfter).toHaveBeenCalledWith(event, response);
  });
});
