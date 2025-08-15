import { expect, test } from "vitest";
import { createLink } from "../create-link";

test("Get link, when no args specified", () => {
  expect(createLink("/test")).toBe("/test");
});

test("Get link, with replaced dynamic parts", () => {
  expect(createLink("/test/%s", "dynamic")).toBe("/test/dynamic");
});

test("Get link, with multiple dynamic parts", () => {
  expect(createLink("/test/%s/subpart/%s", "dynamic1", "dynamic2")).toBe(
    "/test/dynamic1/subpart/dynamic2"
  );
});
