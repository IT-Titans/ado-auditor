<script setup lang="ts">
import type { RecommendationItem } from "#shared/types/types";

const recommendations = ref<RecommendationItem[]>([
  {
    text: "A terrific piece of praise",
    user: {
      name: "Gundula Gause",
      position: "Project Manager",
    },
  },
  {
    text: "A fantastic bit of feedback",
    user: {
      name: "Klaus Peter",
      position: "Cyber Sec Officer",
    },
  },
  {
    text: "A genuinely glowing review",
    user: {
      name: "Max Mertens",
      position: "Consultant",
    },
  },
]);
</script>

<template>
  <div class="flex flex-col gap-4 w-full">
    <h3 class="font-bold text-3xl">Recommendations</h3>
    <div class="flex gap-4 w-full">
      <RecommendationCard
        v-for="item in recommendations"
        :key="item.text"
        :item
      />
    </div>
  </div>
</template>

<style scoped></style>
