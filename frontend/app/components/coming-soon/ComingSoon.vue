<script setup lang="ts">
import {usedIcons} from "~/constants/icons";

defineProps<{
  scope: Scope;
  featureTitle?: string;
  iconName?: string;
}>()
</script>

<template>
      <h1 class="text-primary text-center font-bold text-4xl">COMING SOON</h1>
      <ComingSoonCard
          :icon="iconName ? iconName : usedIcons[scope].default"
          :feature-title
      />
</template>

<style scoped>

</style>