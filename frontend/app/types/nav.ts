import type {LucideIcon} from "lucide-vue-next";

export interface NavigationGroup {
  groupTitle: string;
  items: Array<NavigationGroupItem>;
}

export interface NavigationGroupItem {
  title: string;
  url: string;
  icon: string | LucideIcon;
  key?: string;
  isActive?: boolean;
  isComingSoon?: boolean;
  items?: Array<NavigationItem>;
}

export interface NavigationItem {
  title: string;
  url: string;
}