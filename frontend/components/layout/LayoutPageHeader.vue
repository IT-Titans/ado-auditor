<script setup lang="ts">
defineProps<{
  title: string;
  totalCounts?: SummaryCounts;
  severity?: Severity;
  showStatusText?: boolean;
}>();
</script>

<template>
  <div class="flex flex-col gap-4">
    <div class="flex justify-between items-center gap-4">
      <h2 class="text-2xl font-bold">
        {{ title }}
      </h2>
      <div v-if="totalCounts" class="flex items-center gap-4">
        <SeverityIcon
          severity="critical"
          :label="
            totalCounts && totalCounts.totalCriticals > 0
              ? totalCounts.totalCriticals
              : '0'
          "
          :icon-color="
            totalCounts && totalCounts.totalCriticals > 0
              ? 'text-severity-critical'
              : undefined
          "
        />
        <SeverityIcon
          severity="warning"
          :label="
            totalCounts && totalCounts.totalWarnings > 0
              ? totalCounts.totalWarnings
              : '0'
          "
          :icon-color="
            totalCounts && totalCounts.totalWarnings > 0
              ? 'text-severity-warning'
              : undefined
          "
        />
        <SeverityIcon
          severity="ok"
          :label="
            totalCounts && totalCounts.totalOks > 0 ? totalCounts.totalOks : '0'
          "
          :icon-color="
            totalCounts && totalCounts.totalOks > 0
              ? 'text-severity-ok'
              : undefined
          "
        />
      </div>
      <div v-if="showStatusText && severity">
        <SeverityIcon
          :severity="severity"
          :icon-color="severityColorOptions[severity]"
        />
      </div>
    </div>
    <USeparator />
  </div>
</template>
