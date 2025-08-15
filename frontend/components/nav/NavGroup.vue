<script setup lang="ts">
import type { NavigationGroup } from '#imports';
import NavComingSoonBadge from "~/components/nav/NavComingSoonBadge.vue";

defineProps<NavigationGroup>();
</script>

<template>
  <SidebarGroup class="pt-0">
    <SidebarGroupLabel>{{ groupTitle }}</SidebarGroupLabel>
    <SidebarMenu>
      <SidebarMenuItem v-for="item in items" :key="item.title">
        <SidebarMenuButton as-child :is-active="item.isActive">
          <ULink :to="item.url" >
              <component :is="item.icon" />
              <span>{{ item.title }}</span>
              <NavComingSoonBadge v-if="item.isComingSoon" />
          </ULink>
        </SidebarMenuButton>
      </SidebarMenuItem>
    </SidebarMenu>
  </SidebarGroup>
</template>
