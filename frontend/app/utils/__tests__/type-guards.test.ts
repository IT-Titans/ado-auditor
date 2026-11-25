import { describe, expect, it } from "vitest";
import { isObject } from "../type-guards";
import { isEmptyObject } from "../utilities";

describe("isObject", () => {
  it("accepts plain objects", () => {
    expect(isObject({})).toBe(true);
    expect(isObject({ a: 1 })).toBe(true);
  });

  it.each([null, undefined, [], [1], "text", 1, true])(
    "rejects %j",
    (value) => {
      expect(isObject(value)).toBe(false);
    }
  );
});

describe("isEmptyObject", () => {
  it("is true only for an object without keys", () => {
    expect(isEmptyObject({})).toBe(true);
    expect(isEmptyObject({ id: "x" })).toBe(false);
  });

  it("is false for non-objects, including empty arrays", () => {
    expect(isEmptyObject([])).toBe(false);
    expect(isEmptyObject(null)).toBe(false);
    expect(isEmptyObject(undefined)).toBe(false);
  });
});
