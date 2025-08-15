<script setup lang="ts">
import { categoriesLabels, categoryLabels } from "~/constants/labels";
import { usedIcons } from "~/constants/icons";
import type { ConcreteComponent } from "vue";

definePageMeta({
  layout: "audit-default",
  middleware: ["check-audit-id"],
});

const usersAffectedTable = resolveComponent("usersAffectedTable");
const patsTable = resolveComponent("PatsTable");

const router = useRouter();
const route = useRoute();
const auditId = route.params.id as string;
const scope = route.params.scope as Scope;
const ruleId = route.params.ruleId as string;

const apiUrl = `/api/rules/${scope}/${ruleId}`;

const componentMap = {
  users: {
    component: usersAffectedTable,
    fetchType: {} as RuleResponse<UserRuleItem>,
  },
  pats: {
    component: patsTable,
    fetchType: {} as RuleResponse<PatRuleItem>,
  },
  projects: {
    component: null,
    fetchType: {} as RuleResponse<ProjectRuleItem>,
  },
  groups: {
    component: null,
    fetchType: {} as RuleResponse<GroupRuleItem>,
  },
} satisfies Record<
  Scope,
  {
    component: string | ConcreteComponent | null;
    fetchType: RuleResponse<
      UserRuleItem | PatRuleItem | ProjectRuleItem | GroupRuleItem
    >;
  }
>;

const selected = componentMap[scope];

const { data, error } = await useAsyncData<typeof selected.fetchType>(
  `rule-${scope}-${ruleId}`,
  () =>
    $fetch(apiUrl, {
      query: {
        auditId: auditId,
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
    label: `${categoryLabels.get(scope)} - Rules`,
    icon: usedIcons.rules.default,
    to: linkTemplates.audit.rules.overview(auditId, scope),
  },
  {
    label: `${categoryLabels.get(scope)} - Rule: ${data.value?.rule.name}`,

    to: linkTemplates.audit.rules.rule(
      auditId,
      scope,
      data.value?.rule.id ?? ""
    ),
  },
]);

const tableComponent = selected.component;
</script>

<template>
  <DataFetchWrapper :error>
    <UBreadcrumb
      v-if="breadcrumbs"
      :items="breadcrumbs"
      class="flex items-center p-4 bg-white h-[50px] border-b"
    />
    <UContainer class="flex flex-1 flex-col py-4 gap-4 overflow-y-auto">
      <LayoutPageHeader
        :title="`${categoryLabels.get(scope)} - Rule: ${data?.rule.name}`"
        :severity="data?.rule.severity"
        :show-status-text="true"
      />
      <h3>Details</h3>
      <UCard>
        <div class="flex flex-col gap-4">
          <div class="line-height-1">
            <Label class="font-semibold">Description</Label>
            <p class="text-sm">{{ data?.rule.description }}</p>
          </div>
          <div>
            <Label class="font-semibold">Recommendation</Label>
            <p class="text-sm">{{ data?.rule.recommendation }}</p>
          </div>
        </div>
      </UCard>
      <h3>Affected {{ categoriesLabels.get(scope) }}</h3>
      <component :is="tableComponent" :data="data?.rule.items" />
    </UContainer>
  </DataFetchWrapper>
</template>
