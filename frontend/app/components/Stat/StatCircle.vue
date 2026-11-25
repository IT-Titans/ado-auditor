<script setup lang="ts">
const { color = "ok" } = defineProps<{
  color?: keyof typeof colorMap;
}>();

const colorMap = {
  critical: "bg-red-600",
  warning: "bg-amber-600",
  ok: "bg-lime-600",
  default: "bg-stone-400",
} as const;
</script>

<template>
  <div
    :class="[
      'w-8 h-8 rounded-full flex items-center justify-center shadow-md',
      colorMap[color] || colorMap.default,
    ]"
  >
    <div class="flex items-center justify-center text-white text-sm">
      <slot />
    </div>
  </div>
</template>

<style scoped></style>
