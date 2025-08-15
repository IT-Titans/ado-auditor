<script setup lang="ts">
defineProps<{
  label?: string;
}>();
</script>

<template>
  <UButton color="neutral" variant="outline">{{ label ?? "View" }}</UButton>
</template>
