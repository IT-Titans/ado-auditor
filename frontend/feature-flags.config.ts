import { defineFeatureFlags } from "#feature-flags/handler";

export default defineFeatureFlags(() => ({
  pats: true,
  users: true,
  projects: false,
  groups: false,
  experimentalFeature: true,
}));
