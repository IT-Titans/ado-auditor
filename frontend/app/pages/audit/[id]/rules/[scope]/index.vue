<script setup lang="ts">
import { categoryLabels } from "~/constants/labels";
import { usedIcons } from "~/constants/icons";

definePageMeta({
  layout: "audit-default",
  middleware: ["check-audit-id"],
});

const router = useRouter();
const route = useRoute();
const {isEnabled} = useFeatureFlags();
const auditId = route.params.id as string;
const scope = route.params.scope as Scope;
const query = route.query;

const { data, error } = await useAsyncData<RulesResponse>(
  `rules-${scope}`,
  () =>
    $fetch<RulesResponse>(`/api/rules/${scope}`, {
      query: {
        auditId,
        ...query,
      },
    }),
  {
    watch: [route],
    immediate: isEnabled(scope),
  }
);

const breadcrumbs = computed(() => [
  {
    icon: usedIcons.home,
    to: linkTemplates.audit.overview(auditId),
  },
  {
    label: `${categoryLabels.get(scope)} - Rules`,
    icon: usedIcons.rules.default,
    to: linkTemplates.audit.rules.overview(auditId, scope),
  },
]);

if (error.value) {
  if (error.value.statusCode === 401 || error.value.statusCode === 410) {
    router.replace(linkTemplates.global.expiredAudit());
  }
}

const pageTitle = computed(() => {
  return `${categoryLabels.get(scope)} - Rules`;
});
</script>

<template>
  <DataFetchWrapper :error="error">
    <UBreadcrumb
      v-if="breadcrumbs"
      :items="breadcrumbs"
      class="flex items-center p-4 bg-white h-[50px] border-b"
    />
    <UContainer class="flex flex-1 flex-col py-4 gap-4 overflow-y-auto">
      <template v-if="isEnabled(scope)">
        <LayoutPageHeader :title="pageTitle" />
        <RulesTable :data="data?.rules ?? []" />
      </template>
      <template v-else>
        <ComingSoon :scope :icon-name="usedIcons.rules.default" :feature-title="pageTitle" />
      </template>
    </UContainer>
  </DataFetchWrapper>
</template>

<style scoped></style>
