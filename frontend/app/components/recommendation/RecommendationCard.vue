<script setup lang="ts">
import type { RecommendationItem } from "#shared/types/types";


defineProps<{
  item: RecommendationItem
}>();

</script>

<template>
  <div class="flex flex-col gap-4 w-full border border-neutral-300 rounded-md p-4">
    <p class="font-bold">“{{ item.text }}”</p>
    <div class='flex gap-2 items-center'>
      <UAvatar :alt="item.user.name" />
      <div class="flex flex-col space-y-1">
        <p class="text-sm font-semibold">{{ item.user.name }}</p>
        <p class="text-xs font-light">{{ item.user.position }}</p>
      </div>
    </div>
  </div>
</template>

<style scoped></style>
