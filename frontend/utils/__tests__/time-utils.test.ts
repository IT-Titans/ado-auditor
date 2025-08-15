import { describe, expect, it } from "vitest";
import { formatDuration } from "../time-utils";

describe("formatDuration", () => {
  it.each([
    { minutes: 1, expected: "1 minute" },
    { minutes: 25, expected: "25 minutes" },
    { minutes: 40, expected: "40 minutes" },
    { minutes: 60, expected: "1 hour" },
    { minutes: 120, expected: "2 hours" },
    { minutes: 180, expected: "3 hours" },
    { minutes: 1380, expected: "23 hours" },
    { minutes: 61, expected: "1 hour and 1 minute" },
    { minutes: 90, expected: "1 hour and 30 minutes" },
    { minutes: 115, expected: "1 hour and 55 minutes" },
    { minutes: 1410, expected: "23 hours and 30 minutes" },
  ])(
    "transforms $minutes into the string: $expected",
    ({ minutes, expected }) => {
      const result = formatDuration(minutes);
      expect(result).toBe(expected);
    }
  );
});
