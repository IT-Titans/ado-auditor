<script setup lang="ts">
import { usedIcons } from "~/constants/icons";
import type { BreadcrumbItem } from "@nuxt/ui";

defineProps<{
  breadcrumbs?: BreadcrumbItem[];
}>();
</script>

<template>
  <UBreadcrumb
    v-if="breadcrumbs"
    :items="breadcrumbs"
    class="flex items-center p-4 bg-white h-[50px] border-b"
  />
  <UContainer class="flex flex-1 flex-col py-4 gap-4 overflow-y-auto">
    <h1 class="text-primary text-center font-bold text-4xl">COMING SOON</h1>
    <ComingSoonCard :icon="usedIcons.groups.default" feature-title="Groups" />
  </UContainer>
</template>

<style scoped></style>
