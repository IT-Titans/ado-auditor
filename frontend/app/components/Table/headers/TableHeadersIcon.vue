<script setup lang="ts">
import { cn } from "~/lib/utils";

defineProps<{
  tooltipText: string;
  icon: string;
  iconColor?: string;
  iconClass?: string;
}>();
</script>

<template>
  <div class="flex items-center justify-center">
    <UTooltip :text="tooltipText">
      <UIcon :name="icon" mode="svg" :class="cn('', iconClass, iconColor)" />
    </UTooltip>
  </div>
</template>
