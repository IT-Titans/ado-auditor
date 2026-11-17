<script setup lang="ts">
defineProps<{
  rings: Ring[];
  display?: {
    text: string;
    class: string;
  };
}>();
</script>

<template>
  <ChartRadialBar :rings class="self-center">
    <template #center>
      <slot />
    </template>
  </ChartRadialBar>
</template>

<style scoped></style>
