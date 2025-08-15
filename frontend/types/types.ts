export type RecommendationItem = {
  text: string;
  user: {
    name: string;
    position: string;
    avatarSrc?: string;
  };
};

export type FeatureItem = {
  id: string;
  title: string;
  body: string;
  imgSrc?: string;
};

export type Category = "pats" | "projects" | "users" | "groups";
export type Scope = "pats" | "projects" | "users" | "groups";

export interface SummaryCounts {
  totalCriticals: number;
  totalWarnings: number;
  totalOks: number;
}

export interface BaseApiResult {
  organizationName: string;
  requestedAt: string;
  createdAt: string;
  validUntil: string;
}

export type FaqSection = {
  question: string;
  answer: string;
};

export interface Ring {
  /** Percentage as 0 → 1 (e.g. 0.78 = 78 %) */
  percent: number;
  /** Progress stroke colour (any valid CSS colour string) */
  strokeClass: string;
  /** Optional track (background) colour */
  trackClass?: string;
}

export interface Stat {
  value: number;
  label: string;
  color?: "critical" | "warning" | "ok" | "default";
  to?: string;
  linkClass?: string;
  class?: string;
}
