export const affectedRulesText = new Map<
  Category,
  Record<"critical" | "warning" | "ok", string>
>([
  [
    "pats",
    {
      critical: "PAT Rules with critical issues",
      warning: "PAT Rules with warnings",
      ok: "PAT Rules are ok",
    },
  ],
  [
    "users",
    {
      critical: "User Rules with critical issues",
      warning: "User Rules with warnings",
      ok: "User Rules are ok",
    },
  ],
  [
    "groups",
    {
      critical: "Group Rules with critical issues",
      warning: "Group Rules with warnings",
      ok: "Group Rules are ok",
    },
  ],
  [
    "projects",
    {
      critical: "Project Rules with critical issues",
      warning: "Project Rules with warnings",
      ok: "Project Rules are ok",
    },
  ],
]);
