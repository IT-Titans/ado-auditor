<script setup lang="ts">
definePageMeta({
  layout: "audit-default",
  middleware: ['check-audit-id']
});

const router = useRouter();
const route = useRoute();

const fetchUrl = computed(() => `/api/audit/${route.params.id}`);
const { data, error } = await useAsyncData<AuditData>(
  `audit-data-${route.params.id}`,
  () => {
    return $fetch(fetchUrl.value, {
      timeout: 1000 * 60 * 10,
    });
  },
  {
    transform: (input) => {
      input.summaries[2] = {
        scope: "projects",
        criticalCount: 0,
        warningCount: 0,
        okCount: 0,
        totalCount: 0,
        items: [
          {
            severity: "critical",
            affectedCount: 0,
            totalCount: 0,
            text: "Projects with weak passwords",
          },
          {
            severity: "warning",
            affectedCount: 0,
            totalCount: 0,
            text: "Projects with outdated MFA",
          },
          {
            severity: "ok",
            affectedCount: 0,
            totalCount: 0,
            text: "Projects with strong security settings",
          },
        ],
      };
      input.summaries[3] = {
        scope: "groups",
        criticalCount: 0,
        warningCount: 0,
        okCount: 0,
        totalCount: 0,
        items: [
          {
            severity: "critical",
            affectedCount: 0,
            totalCount: 0,
            text: "Users with weak passwords",
          },
          {
            severity: "warning",
            affectedCount: 0,
            totalCount: 0,
            text: "Users with outdated MFA",
          },
          {
            severity: "ok",
            affectedCount: 0,
            totalCount: 0,
            text: "Users with strong security settings",
          },
        ],
      };
      return input;
    },
  }
);

if (error.value) {
  console.error("Error fetching audit data: ", error.value);
  if (error.value.statusCode === 410) {
    router.replace(linkTemplates.global.expiredAudit());
  }
}
</script>

<template>
  <DataFetchWrapper :error="error">
    <UContainer class="flex flex-1 flex-col py-4 gap-4">
      <SummaryContainer :audit-categories="data?.summaries ?? []" />
      <AuditValidTimer
        :requested-at="data?.requestedAt ?? ''"
        :valid-until="data?.validUntil ?? ''"
        :organization-name="data?.organizationName ?? ''"
      />
    </UContainer>
  </DataFetchWrapper>
</template>

<style scoped></style>
