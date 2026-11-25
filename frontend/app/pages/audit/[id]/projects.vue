<script setup lang="ts">
import type { BreadcrumbItem } from "@nuxt/ui";

definePageMeta({
  layout: "audit-default",
  middleware: ["check-audit-id"],
});

const route = useRoute();
const auditId = route.params.id as string;

const breadcrumbs = computed<BreadcrumbItem[]>(() => [
  {
    icon: "i-lucide-home",
    to: createLink("/audit/%s", auditId),
  },
  {
    label: `Projects`,
    icon: "i-lucide-inbox",
    to: createLink("/audit/%s/projects", auditId),
  },
]);
</script>

<template>
  <ProjectsComingSoon :breadcrumbs />
</template>

<style scoped></style>
