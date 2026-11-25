import { describe, expect, it } from "vitest";
import {
  errorMassages,
  getErrorMessage,
  getErrorTitle,
} from "../error-messages";

describe("getErrorMessage", () => {
  it("returns the configured message for a known status code", () => {
    expect(getErrorMessage(404)).toBe(errorMassages[404]?.message);
  });

  it("falls back when no status code is given", () => {
    expect(getErrorMessage()).toBe("An unknown error has occurred.");
    expect(getErrorMessage(0)).toBe("An unknown error has occurred.");
  });

  it("falls back instead of throwing for a status code without an entry", () => {
    expect(getErrorMessage(418)).toBe("An unknown error has occurred.");
  });
});

describe("getErrorTitle", () => {
  it("returns the configured title for a known status code", () => {
    expect(getErrorTitle(500)).toBe(errorMassages[500]?.title);
  });

  it("falls back when no status code is given", () => {
    expect(getErrorTitle()).toBe("Unknown error!");
  });

  it("falls back instead of throwing for a status code without an entry", () => {
    expect(getErrorTitle(418)).toBe("Unknown error!");
  });
});

describe("errorMassages", () => {
  it.each(Object.entries(errorMassages))(
    "status %s has a title and a message",
    (_code, content) => {
      expect(content.title).not.toBe("");
      expect(content.message).not.toBe("");
    }
  );
});
