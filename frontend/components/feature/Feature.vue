<script setup lang="ts">
const features = ref<Feature[]>([
  {
    id: "1",
    title: "Insights",
    body: "Get clear Security Insights that highlight risks, misconfigurations, and areas needing attention across your Azure DevOps organization.",
    imgSrc: "/images/insight-preview.png",
  },
  {
    id: "2",
    title: "Is your intern an admin?",
    body: "Quickly identify which of your users have admin access, helping you prevent unauthorized control over your resources.",
    imgSrc: "/images/admin-acces-image.png",
  },
  {
    id: "3",
    title: "Reports",
    body: "Receive automatically generated reports on a regular schedule, giving you consistent visibility into your organization's security posture.",
    imgSrc: "/images/reports-preview.png",
  },
]);
</script>

<template>
  <section class="flex flex-col gap-4">
    <h3 class="font-bold text-3xl">Features</h3>
    <div class="flex gap-4 w-full">
      <FeatureItem v-for="item in features" :key="item.id" :item />
    </div>
  </section>
</template>

<style scoped></style>
