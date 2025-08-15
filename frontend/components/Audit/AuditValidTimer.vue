<script setup lang="ts">
const props = defineProps<{
  requestedAt: string;
  validUntil: string;
  organizationName: string;
}>();

const formattedDate = useDateFormat(props.requestedAt, "DD.MM.YYYY HH:mm", {
  locales: "de",
});

const minutesLeft = useTimeAgo(props.validUntil, { updateInterval: 15_000 });

const expireText = computed(() => {
  if (minutesLeft.value.includes("ago")) {
    return `expired ${minutesLeft.value}`;
  }
  return `expires ${minutesLeft.value}`;
});
</script>

<template>
  <section class="text-lg text-right text-gray-400 italic">
    <ULink
      target="_blank"
      :to="`https://dev.azure.com/${organizationName}/`"
      class="hover:underline"
    >
      <p>
        Audit for Azure DevOps organization
        <span class="font-bold">{{ organizationName }}</span>
        from
        <span class="font-bold">{{ formattedDate }}</span>
      </p>
      <p>Audit data is not persisted. This audit {{ expireText }}.</p>
    </ULink>
  </section>
</template>
