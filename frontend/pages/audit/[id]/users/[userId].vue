<script setup lang="ts">
import type { AccordionItem, TabsItem } from "@nuxt/ui";
import { usedIcons } from "~/constants/icons";

definePageMeta({
  layout: "audit-default",
  title: "User details",
});

const router = useRouter();
const route = useRoute();
const auditId = route.params.id as string;

const { data, error } = await useAsyncData<UserDetailsResponse>(
  "userDetails" + route.params.userId,
  () =>
    $fetch(`/api/users/${route.params.userId}`, {
      query: {
        auditId: route.params.id,
      },
    })
);

if (error.value) {
  if (error.value.statusCode === 401 || error.value.statusCode === 410) {
    router.replace(linkTemplates.global.expiredAudit());
  }
}

const items = [
  {
    label: "Projects",
    value: "projects",
    description: "View and manage projects associated with this user.",
    icon: usedIcons.projects.default,
    slot: "projects" as const,
  },
  {
    label: "Groups",
    value: "groups",
    description: "View and manage groups associated with this user.",
    icon: usedIcons.groups.default,
    slot: "groups" as const,
  },
  {
    label: "PATs",
    value: "pats",
    description:
      "Make changes to your account here. Click save when you're done.",
    icon: usedIcons.pats.default,
    slot: "pats" as const,
  },
] satisfies TabsItem[];

const activeTab = computed({
  get() {
    return (route.query.tab as string) || "pats";
  },
  set(tab: string) {
    router.push({
      query: { tab },
    });
  },
});


const overallTotalSummarySeverity = computed<Severity | undefined>(() => {
  const summaries = [data.value?.patsSummary, data.value?.groupsSummary, data.value?.projectsSummary];

  for (const summary of summaries) {
    if (summary && summary.totalCriticals > 0) return  'critical';
    if (summary && summary.totalWarnings > 0) return 'warning';
    if (summary && summary.totalOks > 0) return 'ok';
  }
  return undefined;
})

const accordionItems = computed(() => ([
  {
    label: "Affected Rules",
    slot: "affected-rules" as const,
    ui: {
      trigger: "text-base",
    },
  },
  {
    label: "Other Resources of User",
    slot: "resources" as const,
    icon: overallTotalSummarySeverity.value && usedIcons.severities[overallTotalSummarySeverity.value],
    ui: {
      label: 'order-0',
      trigger: "text-base",
      leadingIcon: ["order-1 text-lg", overallTotalSummarySeverity.value && 'text-severity-' + overallTotalSummarySeverity.value],
      trailingIcon: ["order-2"],
    },
  },
] satisfies AccordionItem[]));

const activeSection = ref(["0"]);

const breadcrumbs = computed(() => [
  {
    icon: usedIcons.home,
    to: linkTemplates.audit.overview(auditId),
  },
  {
    label: "Users",
    icon: usedIcons.users.default,
    to: linkTemplates.audit.users.overview(auditId),
  },
  {
    label: `User: ${data?.value?.user.userName}`,
    to: linkTemplates.audit.users.user(auditId, data.value?.user.id ?? "#"),
  },
]);

const pageTitle = computed(() => {
  return data && data.value ? `User: ${data.value.user.userName}` : "";
});

const totalRulesCounts = computed(() => {
  const totals: SummaryCounts = {
    totalCriticals: 0,
    totalWarnings: 0,
    totalOks: 0,
  };

  if (data.value) {
    for (const rule of data.value.rules) {
      switch (rule.severity) {
        case "critical":
          totals.totalCriticals += 1;
          break;
        case "warning":
          totals.totalWarnings += 1;
          break;
        case "ok":
          totals.totalOks += 1;
          break;
      }
    }
  }
  return totals;
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
      <LayoutPageHeader :title="pageTitle" :total-counts="totalRulesCounts" />
      <h3>Details</h3>
      <div class="flex gap-4">
        <UCard class="flex-1">
          <div class="grid grid-cols-2">
            <div class="flex flex-col gap-2">
              <div class="flex gap-2">
                <Label class="font-semibold">Name:</Label>
                <p class="text-sm">{{ data?.user.userName }}</p>
              </div>
              <div class="flex gap-2">
                <Label class="font-semibold">Type:</Label>
                <p class="text-sm capitalize">{{ data?.user.metaType }}</p>
              </div>
            </div>
            <div class="flex flex-col gap-2">
              <div class="flex items-center gap-2">
                <Label class="font-semibold">Status:</Label>
                <div class="flex items-center gap-2">
                  <p class="text-sm">
                    {{ data?.user.isActive ? "Active" : "Inactive" }}
                  </p>
                  <UIcon name="i-tabler-user-check" :class="['size-6']" />
                </div>
              </div>
            </div>
          </div>
        </UCard>
      </div>
      <UAccordion
        v-model="activeSection"
        type="multiple"
        :items="accordionItems"
        :ui="{
          trigger: 'font-normal',
          label: 'text-xl font-bold text-gray-900 dark:text-white',
        }"
      >
        <template #affected-rules-body>
          <RulesAffectedTable :data="data?.rules ?? []" scope="users" />
        </template>
        <template #resources-body>
          <UTabs v-model="activeTab" :items class="gap-4">
            <template #projects>
              <div>
                <ComingSoonCard
                  feature-title="Projects"
                  :icon="usedIcons.projects.default"
                />
              </div>
            </template>
            <template #groups>
              <div>
                <ComingSoonCard
                  feature-title="Groups"
                  :icon="usedIcons.groups.default"
                />
              </div>
            </template>
            <template #pats>
              <PatsTable
                :data="data?.pats || []"
                :column-visibility="{ user_userName: false }"
              />
            </template>
          </UTabs>
        </template>
      </UAccordion>
    </UContainer>
  </DataFetchWrapper>
</template>

<style scoped></style>
