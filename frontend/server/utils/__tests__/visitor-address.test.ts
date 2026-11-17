import { describe, expect, it } from "vitest";
import { resolveVisitorAddress } from "../visitor-address";

describe("resolveVisitorAddress", () => {
  describe("with a forwarded header", () => {
    it("takes the last entry, which is the one the platform appended", () => {
      expect(resolveVisitorAddress("1.1.1.1, 2.2.2.2:443", undefined)).toBe(
        "2.2.2.2"
      );
    });

    it("ignores an address the visitor put in front of the real one", () => {
      // A visitor sending its own X-Forwarded-For must not get to choose the address its
      // limit is counted under.
      expect(
        resolveVisitorAddress("203.0.113.99, 198.51.100.7:51234", "10.0.0.1")
      ).toBe("198.51.100.7");
    });

    it("accepts a single entry without a port", () => {
      expect(resolveVisitorAddress("198.51.100.7", undefined)).toBe(
        "198.51.100.7"
      );
    });

    it("strips the port from a bracketed IPv6 entry", () => {
      expect(resolveVisitorAddress("[2001:db8::1]:51234", undefined)).toBe(
        "2001:db8::1"
      );
    });

    it("accepts a bracketed IPv6 entry without a port", () => {
      expect(resolveVisitorAddress("[2001:db8::1]", undefined)).toBe(
        "2001:db8::1"
      );
    });

    it("leaves a bare IPv6 entry as it is", () => {
      expect(resolveVisitorAddress("1.1.1.1, 2001:db8::1", undefined)).toBe(
        "2001:db8::1"
      );
    });

    it("tolerates whitespace around the entry", () => {
      expect(resolveVisitorAddress("1.1.1.1 ,  2.2.2.2:443  ", undefined)).toBe(
        "2.2.2.2"
      );
    });
  });

  describe("without a usable forwarded header", () => {
    it("falls back to the socket address when the header is missing", () => {
      expect(resolveVisitorAddress(undefined, "127.0.0.1")).toBe("127.0.0.1");
    });

    it("falls back to the socket address when the header is empty", () => {
      expect(resolveVisitorAddress("", "127.0.0.1")).toBe("127.0.0.1");
    });

    it("falls back to the socket address when the last entry is not an address", () => {
      expect(resolveVisitorAddress("1.1.1.1, unknown", "127.0.0.1")).toBe(
        "127.0.0.1"
      );
    });

    it("does not fall back to an earlier entry of the header", () => {
      // Earlier entries come from the caller. Using one because the last is unreadable
      // would reopen the door the last-entry rule closes.
      expect(resolveVisitorAddress("1.1.1.1, unknown", undefined)).toBeUndefined();
    });

    it("returns nothing when neither source holds an address", () => {
      expect(resolveVisitorAddress(undefined, undefined)).toBeUndefined();
      expect(resolveVisitorAddress("garbage", "also-garbage")).toBeUndefined();
    });
  });
});
