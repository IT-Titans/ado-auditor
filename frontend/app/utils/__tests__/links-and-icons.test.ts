import { describe, expect, it } from "vitest";
import { linkTemplates } from "../create-link";
import { severityIcons } from "../icon-utils";

describe("linkTemplates", () => {
  it("builds the static global links", () => {
    expect(linkTemplates.global.home()).toBe("/");
    expect(linkTemplates.global.audit()).toBe("/audit");
    expect(linkTemplates.global.expiredAudit()).toBe("/audit?expired=true");
  });

  it("builds the audit links with their identifiers", () => {
    const { audit } = linkTemplates;
    expect(audit.overview("a1")).toBe("/audit/a1");
    expect(audit.expired()).toBe("/audit/expired");
    expect(audit.pats.overview("a1")).toBe("/audit/a1/pats");
    expect(audit.pats.pat("a1", "p2")).toBe("/audit/a1/pats/p2");
    expect(audit.users.overview("a1")).toBe("/audit/a1/users");
    expect(audit.users.user("a1", "u2")).toBe("/audit/a1/users/u2");
    expect(audit.rules.overview("a1", "projects" as Scope)).toBe(
      "/audit/a1/rules/projects"
    );
    expect(audit.rules.rule("a1", "projects" as Scope, "r9")).toBe(
      "/audit/a1/rules/projects/r9"
    );
  });
});

describe("severityIcons", () => {
  it("has a distinct icon per severity", () => {
    const icons = Object.values(severityIcons);
    expect(Object.keys(severityIcons).sort()).toEqual([
      "critical",
      "ok",
      "warning",
    ]);
    expect(new Set(icons).size).toBe(icons.length);
  });
});
