<script setup lang="ts">
import { usedIcons } from "~/constants/icons";
import type { BreadcrumbItem } from "@nuxt/ui";

defineProps<{
  breadcrumbs?: BreadcrumbItem[];
}>();
</script>

<template>
  <div class="flex flex-1 flex-col">
    <UBreadcrumb
      v-if="breadcrumbs"
      :items="breadcrumbs"
      class="flex items-center p-4 bg-white h-[50px] border-b"
    />
    <UContainer class="flex flex-1 flex-col py-4 gap-4 overflow-y-auto">
      <h1 class="text-primary text-center font-bold text-4xl">COMING SOON</h1>
      <ComingSoonCard
        :icon="usedIcons.projects.default"
        feature-title="Projects"
      />
    </UContainer>
  </div>
</template>

<style scoped></style>
