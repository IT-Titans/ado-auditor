<script setup lang="ts">
import { usedIcons } from "~/constants/icons";
import { categoryLabels } from "~/constants/labels";
import { affectedRulesText } from "~/constants/texts";

const props = defineProps<{
  category: Category;
  summary: SummaryCounts;
  totals: number;
}>();

const rings = computed(() => {
  if (props.totals === 0) {
    return [];
  }

  const criticalPercent = props.summary.totalCriticals / props.totals;
  const warningPercent = props.summary.totalWarnings / props.totals;
  const okPercent = props.summary.totalOks / props.totals;

  return [
    {
      percent: okPercent,
      strokeClass: "stroke-lime-500",
      trackClass: okPercent > 0 ? "stroke-lime-200" : undefined,
    },
    {
      percent: warningPercent,
      strokeClass: "stroke-amber-600",
      trackClass: warningPercent > 0 ? "stroke-amber-200" : undefined,
    },
    {
      percent: criticalPercent,
      strokeClass: "stroke-red-600",
      trackClass: criticalPercent > 0 ? "stroke-red-200" : undefined,
    },
  ] as Ring[];
});

const stats: ComputedRef<Array<Stat>> = computed(() => {
  if (props.totals === 0) {
    return [];
  }

  return [
    {
      value: props.summary.totalCriticals,
      label: affectedRulesText.get(props.category)?.critical ?? "",
      color: props.summary.totalCriticals > 0 ? "critical" : "default",
    },
    {
      value: props.summary.totalWarnings,
      label: affectedRulesText.get(props.category)?.warning ?? "",
      color: props.summary.totalWarnings > 0 ? "warning" : "default",
    },
    {
      value: props.summary.totalOks,
      label: affectedRulesText.get(props.category)?.ok ?? "",
      color: props.summary.totalOks > 0 ? "ok" : "default",
    },
  ];
});

const display = computed(() => {
  if (props.summary.totalCriticals > 0) {
    return {
      text: props.summary.totalCriticals + "/" + props.totals,
      class: "text-red-600",
    };
  }
  if (props.summary.totalWarnings > 0) {
    return {
      text: props.summary.totalWarnings + "/" + props.totals,
      class: "text-amber-600",
    };
  }
  return {
    text: props.summary.totalOks + "/" + props.totals,
    class: "text-lime-600",
  };
});
</script>

<template>
  <UCard>
    <template #header>
      <ULink
        :to="`${$route.path}?tab=${category}`"
        class="text-lg font-semibold flex gap-2 items-center"
        :exact-hash="true"
        :exact-query="true"
      >
        <UIcon mode="svg" :name="usedIcons[category].default" />
        {{ categoryLabels.get(category) + " Rules" }}
      </ULink>
    </template>
    <div class="flex flex-col md:flex-row items-center gap-4">
      <StatGauge :rings>
        <span :class="['text-xl font-semibold', display.class]">
          {{ display.text }}
        </span>
      </StatGauge>
      <StatList :stats></StatList>
    </div>
  </UCard>
</template>

<style scoped></style>
