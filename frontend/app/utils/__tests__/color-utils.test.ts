import { describe, expect, it } from "vitest";
import { severityColorOptions } from "../color-utils";

describe("SeverityColors", () => {
  it.each([
    { severity: "ok" as Severity, expected: "text-severity-ok" },
    { severity: "warning" as Severity, expected: "text-severity-warning" },
    { severity: "critical" as Severity, expected: "text-severity-critical" },
  ])(
    "Get for the severity: $severity, the right color: $expected",
    ({ severity, expected }) => {
      const result = severityColorOptions[severity];

      expect(result).toBe(expected);
    }
  );
});
