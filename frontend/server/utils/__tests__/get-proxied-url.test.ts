import { describe, it, expect, vi, beforeEach } from "vitest";
import { getProxiedUrl } from "../get-proxied-url"; // adjust the path as needed
import type { H3Event } from "h3";

const { useRuntimeConfigMock } = vi.hoisted(() => {
  return {
    useRuntimeConfigMock: vi.fn(() => {
      return {
        apiUrl: "https://api.example.com",
      };
    }),
  };
});

// Nitro provides useRuntimeConfig to server code as an auto-import. Without the server
// build the function under test looks it up as a global, which is where the mock goes.
vi.stubGlobal("useRuntimeConfig", useRuntimeConfigMock);

describe("getProxiedUrl", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should return the full proxied URL when apiUrl is set", () => {
    // Arrange

    const event = {
      path: "/test/endpoint",
    } as H3Event;

    // Act
    const result = getProxiedUrl(event);

    // Assert
    expect(result).toBe("https://api.example.com/test/endpoint");
  });
  it.each([
    {
      query: "?auditId=abcde",
      expected: "https://api.example.com/test?auditId=abcde",
    },
    {
      query: "?auditId=abcde&severity=pats",
      expected: "https://api.example.com/test?auditId=abcde&severity=pats",
    },
  ])(
    "should return the full proxied URL with query param $query",
    ({ query, expected }) => {
      const event = {
        path: "/test" + query,
      } as H3Event;

      const result = getProxiedUrl(event);

      expect(result).toBe(expected);
    }
  );
  it("should throw an error when apiUrl is not configured", () => {
    const event = {
      path: "/test",
    } as H3Event;

    useRuntimeConfigMock.mockImplementation(() => {
      return { apiUrl: "" };
    });
    // Act & Assert
    expect(() => getProxiedUrl(event)).toThrowError(
      "API URL is not configured in runtime config"
    );
  });
});
