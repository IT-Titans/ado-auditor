import { describe, expect, it } from "vitest";

// nuxt-feature-flags answers /api/_feature-flags/feature-flags from the public runtime
// configuration and from nowhere else. Flags that do not arrive there read as off in the
// whole application, without an error: the rules pages for PATs and users then show
// "Coming soon" instead of their tables.
describe("feature flags", () => {
  it("reach the runtime configuration the server answers from", () => {
    const flags = useRuntimeConfig().public.featureFlags.flags;

    expect(flags).toMatchObject({ pats: true, users: true, projects: false, groups: false });
  });
});
