<script setup lang="ts">
import { usedIcons } from "~/constants/icons";

definePageMeta({
  layout: "audit-default",
  middleware: ["check-audit-id"],
});

const router = useRouter();
const route = useRoute();
const auditId = route.params.id as string;

const { data, error } = await useAsyncData<PatResponse>(`pats`, () =>
  $fetch(`/api/pats`, {
    query: {
      auditId,
    },
  })
);

if (error.value) {
  if (error.value.statusCode === 401 || error.value.statusCode === 410) {
    router.replace(linkTemplates.global.expiredAudit());
  }
}

const breadcrumbs = computed(() => [
  {
    icon: usedIcons.home,
    to: linkTemplates.audit.overview(auditId),
  },
  {
    label: "PATs",
    icon: usedIcons.pats.default,
    to: linkTemplates.audit.pats.overview(auditId),
  },
]);
</script>

<template>
  <DataFetchWrapper :error="error">
    <UBreadcrumb
      v-if="breadcrumbs"
      :items="breadcrumbs"
      class="flex items-center p-4 bg-white h-[50px] border-b"
    />
    <UContainer class="flex flex-1 flex-col py-4 gap-4">
      <LayoutPageHeader title="PATs" />
      <PatsTable :data="data?.pats ?? []" />
    </UContainer>
  </DataFetchWrapper>
</template>

<style scoped></style>
