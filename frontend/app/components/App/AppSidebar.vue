<script setup lang="ts">
import { House } from "lucide-vue-next";
import { navIcons, navMeta } from "@/constants/navMeta";
import type { SidebarProps } from "../sidebar";
import NavGroup from "../nav/NavGroup.vue";

const route = useRoute();

const id = route.params.id;

const props = withDefaults(defineProps<SidebarProps>(), {
  collapsible: "icon",
});

// Check if current route is the audit overview page
const isAuditOverview = computed(() => {
  return route.path === `/audit/${id}`;
});

const navGroups = {
  details: [
    {
      key: "pats",
      title: navMeta.pats.title,
      url: `/audit/${id}/pats`,
      icon: navMeta.pats.icon,
    },
    {
      key: "users",
      title: navMeta.users.title,
      url: `/audit/${id}/users`,
      icon: navMeta.users.icon,
    },
    {
      key: "projects",
      title: navMeta.projects.title,
      url: `/audit/${id}/projects`,
      icon: navMeta.projects.icon,
      isComingSoon: true,
    },
    {
      key: "groups",
      title: navMeta.groups.title,
      url: `/audit/${id}/groups`,
      icon: navMeta.groups.icon,
      isComingSoon: true,
    },
  ],
  rules: [
    {
      key: "patRules",
      title: "PAT Rules",
      url: `/audit/${id}/rules/pats`,
      icon: navIcons.patRules,
    },
    {
      key: "userRules",
      title: "User Rules",
      url: `/audit/${id}/rules/users`,
      icon: navIcons.userRules,
    },
    {
      key: "projectRules",
      title: "Project Rules",
      url: `/audit/${id}/rules/projects`,
      icon: navIcons.projectRules,
      isComingSoon: true,
    },
    {
      key: "groupRules",
      title: "Group Rules",
      url: `/audit/${id}/rules/groups`,
      icon: navIcons.groupRules,
      isComingSoon: true,
    }
  ],
};

// Computed navMain with isActive based on current route
const navMain = computed(() =>
  navGroups.details.map((item) => ({
    ...item,
    isActive: route.path.includes(item.url),
  }))
);

const navRules = computed(() =>
  navGroups.rules.map((item) => ({
    ...item,
    isActive: route.path.includes(item.url),
  }))
);
</script>

<template>
  <Sidebar v-bind="props" class="fixed top-16">
    <SidebarContent class="overflow-hidden bg-white">
      <SidebarGroup>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton as-child :is-active="isAuditOverview">
              <ULink :to="`/audit/${id}`">
                <House />
                <span>Audit Overview</span>
              </ULink>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarGroup>
      <NavGroup group-title="Details" :items="navMain" />
      <NavGroup group-title="Rules" :items="navRules" />
    </SidebarContent>
    <SidebarFooter class="bg-white">
      <FeedbackSendModal />
    </SidebarFooter>
  </Sidebar>
</template>
