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
export const navIcons: Record<string, LucideIcon> = {
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
};

// Define a mapping from nav item keys to display titles
export const navTitles: Record<string, string> = {
  home: "Home",
  projects: "Projects",
  groups: "Groups",
  users: "Users",
  user: "User",
  pats: "PATs",
  patRules: "PAT Rules",
  userRules: "User Rules",
};

// Optionally, export both together for convenience
export const navMeta = Object.keys(navIcons).reduce((acc, key) => {
  acc[key] = {
    icon: navIcons[key],
    title: navTitles[key],
  };
  return acc;
}, {} as Record<string, { icon: LucideIcon; title: string }>);
