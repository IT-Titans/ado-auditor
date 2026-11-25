import type { LucideIcon } from "lucide-vue-next";

import {
  House,
  Inbox,
  Users,
  User,
  KeyRound,
  Group,
  Scale,
} from "lucide-vue-next";

// Define a mapping from nav item keys to icon components
export const navIcons = {
  home: House,
  projects: Inbox,
  groups: Group,
  users: Users,
  user: User,
  pats: KeyRound,
  patRules: Scale,
  userRules: Scale,
  projectRules: Scale,
  groupRules: Scale,
} satisfies Record<string, LucideIcon>;

export type NavKey = keyof typeof navIcons;

// Define a mapping from nav item keys to display titles
export const navTitles: Record<NavKey, string> = {
  home: "Home",
  projects: "Projects",
  groups: "Groups",
  users: "Users",
  user: "User",
  pats: "PATs",
  patRules: "PAT Rules",
  userRules: "User Rules",
  projectRules: "Project Rules",
  groupRules: "Group Rules",
};

// Optionally, export both together for convenience
export const navMeta = (Object.keys(navIcons) as NavKey[]).reduce((acc, key) => {
  acc[key] = {
    icon: navIcons[key],
    title: navTitles[key],
  };
  return acc;
}, {} as Record<NavKey, { icon: LucideIcon; title: string }>);
