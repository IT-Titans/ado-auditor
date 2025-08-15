<script setup lang="ts">
import { SummaryCard } from "#components";

defineProps<{
  auditCategories: AuditCategory[];
  orientation?: "horizontal" | "vertical";
}>();
</script>

<template>
  <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
    <SummaryCard
      v-for="item in auditCategories"
      :key="item.scope"
      :scope="item.scope"
      :critical-count="item.criticalCount"
      :warning-count="item.warningCount"
      :ok-count="item.okCount"
      :total-count="item.totalCount"
      :orientation
    />
  </div>
</template>

<style scoped></style>
