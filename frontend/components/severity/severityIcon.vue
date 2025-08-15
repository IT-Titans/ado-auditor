<script setup lang="ts">
const props = defineProps<{
  severity: Severity;
  iconColor?: SeverityColors;
  label?: string | number;
}>();

const content = computed(() => (props.label ? props.label : props.severity));
</script>

<template>
  <div class="flex justify-items-center items-center gap-2">
    <UIcon
      :name="severityIcons[severity]"
      class="size-6"
      :class="iconColor ? iconColor : 'text-stone-400'"
      mode="svg"
    />
    <span class="capitalize">{{ content }}</span>
  </div>
</template>
