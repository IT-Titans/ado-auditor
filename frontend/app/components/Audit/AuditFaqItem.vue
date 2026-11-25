<script setup lang="ts">
defineProps<{
  item: FaqSection;
}>();
</script>

<template>
  <article class="flex flex-col gap-2">
    <h4 class="text-lg font-semibold">{{ item.question }}</h4>
    <!-- TODO: Replace with better solution can lead to XXS Attacks -->
    <p class="text-sm text-gray-700" v-html="item.answer" />
  </article>
</template>

<style scoped></style>
