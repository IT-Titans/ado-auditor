<script setup lang="ts">
import type { BreadcrumbItem } from "@nuxt/ui";
import { usedIcons } from "~/constants/icons";

definePageMeta({
  layout: "audit-default",
  middleware: ["check-audit-id"],
});

const route = useRoute();
const auditId = route.params.id as string;

const breadcrumbs = computed<BreadcrumbItem[]>(() => [
  {
    icon: usedIcons.home,
    to: createLink("/audit/%s", auditId),
  },
  {
    label: `Groups`,
    icon: usedIcons.groups.default,
    to: createLink("/audit/%s/groups", auditId),
  },
]);
</script>

<template>
  <div class="flex flex-1 flex-col">
    <GroupsComingSoon :breadcrumbs />
  </div>
</template>

<style scoped></style>
