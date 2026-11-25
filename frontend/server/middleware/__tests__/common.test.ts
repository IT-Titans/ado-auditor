import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

type Handler = (event: unknown) => Promise<void>;

describe("server access log middleware", () => {
  let handler: Handler;

  beforeEach(async () => {
    vi.stubGlobal("defineEventHandler", (fn: Handler) => fn);
    vi.resetModules();
    handler = (await import("../01.common")).default as unknown as Handler;
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  it("logs method, url, status and duration once the response finished", async () => {
    const log = vi.spyOn(console, "log").mockImplementation(() => {});
    let onFinish: () => void = () => {};
    const res = {
      statusCode: 204,
      on: vi.fn((_: string, cb: () => void) => {
        onFinish = cb;
      }),
    };

    await handler({ node: { req: { method: "GET", url: "/api/x" }, res } });
    expect(res.on).toHaveBeenCalledWith("finish", expect.any(Function));
    expect(log).not.toHaveBeenCalled();

    onFinish();
    expect(log).toHaveBeenCalledTimes(1);
    expect(log.mock.calls[0]?.[0]).toMatch(/^\[.+\] GET \/api\/x 204 - \d+ms$/);
  });
});
