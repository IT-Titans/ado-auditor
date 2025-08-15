<script setup lang="ts">
import { usedIcons } from "~/constants/icons";

definePageMeta({
  layout: "audit-default",
  title: "PAT Detail",
  middleware: ["check-audit-id"],
});

const router = useRouter();
const route = useRoute();
const auditId = route.params.id as string;
const padId = route.params.patId as string;

const fetchApi = computed(() => `pat-${padId}`);

const { data, status, error } = await useAsyncData<PatDetailResponse>(
  fetchApi,
  () =>
    $fetch("/api/pats/" + padId, {
      query: {
        auditId: route.params.id,
      },
      onResponseError: ({ response }) => {
        throw createError({
          statusCode: response.status,
          statusMessage: response.statusText,
          message: "Error occurred",
        });
      },
    })
);

if (error.value) {
  if (error.value.statusCode === 401 || error.value.statusCode === 410) {
    router.replace(linkTemplates.global.expiredAudit());
  }
}

const createdAt = useTimeAgo(data.value?.pat.createdAt || "");

const expiredIn = useTimeAgo(data.value?.pat.expiresIn || "");

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
  {
    label: `PAT: ${data?.value?.pat.name}`,
    to: linkTemplates.audit.pats.pat(auditId, data.value?.pat.id ?? ""),
  },
]);
const pageTitle = computed(() => {
  return data && data.value ? `PAT: ${data.value.pat.name}` : "";
});
</script>

<template>
  <DataFetchWrapper :error="error" :pending="status === 'pending'">
    <UBreadcrumb
      :items="breadcrumbs"
      class="flex items-center p-4 bg-white h-[50px] border-b"
    />
    <UContainer class="flex flex-1 flex-col py-4 gap-4">
      <LayoutPageHeader
        :title="pageTitle"
        :total-counts="data?.pat.affectedPatRulesCounts"
      />
      <h3>Details</h3>
      <UCard>
        <div class="grid grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <div class="flex gap-2">
              <Label class="font-semibold">Name:</Label>
              <p class="text-sm">{{ data?.pat.name }}</p>
            </div>
            <div class="flex gap-2 flex-wrap">
              <Label class="font-semibold">Scopes:</Label>
              <UBadge
                v-for="scope in data?.pat.scopes"
                :key="scope"
                variant="outline"
                color="neutral"
              >
                {{ scope }}
              </UBadge>
            </div>
            <div class="flex gap-2">
              <Label class="font-semibold">Created By:</Label>
              <ULink
                :to="
                  '/audit/' +
                  $route.params.id +
                  '/users/' +
                  data?.pat.user?.userId
                "
                class="text-sm"
                >{{ data?.pat.user?.userName }}</ULink
              >
            </div>
          </div>
          <div class="flex flex-col gap-2">
            <div class="flex gap-2">
              <Label class="font-semibold">Valid for Organization:</Label>
              <p class="text-sm">{{ data?.organizationName }}</p>
            </div>
            <div class="flex gap-2">
              <Label class="font-semibold text-right">Created:</Label>
              <p class="text-sm">{{ createdAt }}</p>
            </div>
            <div class="flex gap-2">
              <Label class="font-semibold text-right">Expires:</Label>
              <p class="text-sm">{{ expiredIn }}</p>
            </div>
          </div>
        </div>
      </UCard>
      <h3>Affected Rules</h3>
      <RulesAffectedTable
        :data="data?.rules ?? []"
        scope="pats"
        :loading="status === 'pending'"
      />
    </UContainer>
  </DataFetchWrapper>
</template>
