<script setup lang="ts">
defineProps<{ status: string }>();
</script>

<template>
  <UBadge color="primary" variant="subtle" size="sm">
    <UIcon name="i-heroicons-clock" class="w-3 h-3 mr-1" />
    {{ status }}
  </UBadge>
</template>
