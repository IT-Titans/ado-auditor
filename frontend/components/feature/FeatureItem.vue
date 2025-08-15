<script setup lang="ts">
defineProps<{
  item: FeatureItem;
}>();
</script>

<template>
  <div class="flex flex-col gap-2 w-full">
    <!-- Replace with image -->
    <img
      :src="item.imgSrc"
      class="h-full w-full object-cover rounded-md"
      alt="Feature image"
    />
    <!--<NuxtImg :src="item.imgSrc" class="h-full w-full rounded-md" />-->
    <!-- <USkeleton class="h-64 w-full" /> -->
    <p class="font-semibold text-lg">{{ item.title }}</p>
    <p>
      {{ item.body }}
    </p>
  </div>
</template>

<style scoped></style>
>
