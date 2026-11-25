<script setup lang="ts">
import { usedIcons } from "~/constants/icons";

defineProps<{
  severity: Severity;
}>();

const color = {
  critical: "text-red-600",
  warning: "text-amber-600",
  ok: "text-lime-600",
};
</script>

<template>
  <UIcon
    v-if="usedIcons.severities[severity]"
    :name="usedIcons.severities[severity]"
    :class="['size-6!', color[severity]]"
    mode="svg"
  />
  <span v-else class="text-gray-500">{{ severity }}</span>
</template>
