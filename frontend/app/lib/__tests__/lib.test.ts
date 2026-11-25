import { describe, expect, it } from "vitest";
import { cn } from "../utils";
import { buttonVariants } from "../ui-variants/button-variants";

describe("cn", () => {
  it("joins class names and drops falsy values", () => {
    expect(cn("a", false, undefined, "b")).toBe("a b");
  });

  it("lets the later Tailwind class win a conflict", () => {
    expect(cn("px-2", "px-4")).toBe("px-4");
  });
});

describe("buttonVariants", () => {
  it("applies the default variant and size", () => {
    const classes = buttonVariants();
    expect(classes).toContain("bg-primary");
    expect(classes).toContain("h-9");
  });

  it("applies the requested variant and size", () => {
    const classes = buttonVariants({ variant: "destructive", size: "lg" });
    expect(classes).toContain("bg-destructive");
    expect(classes).toContain("h-10");
    expect(classes).not.toContain("bg-primary");
  });
});
