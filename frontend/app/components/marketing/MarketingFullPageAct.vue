<script setup lang="ts">
import {categoryLabels} from "~/constants/labels";

const scope = "users";
const data =
    {
      "id": "1",
      "scope": "users",
      "severity": "warning" as Severity,
      "name": "User has more than 3 PATs",
      "description": "A user with an excessive number of Personal Access Tokens (PATs), poses a significant security and management risk. Each PAT represents a potential entry point into your Azure DevOps environment, and the more tokens that exist, the harder it becomes to track, audit, and revoke them when necessary. This increases the likelihood of forgotten, misused, or compromised tokens remaining active and unnoticed, violating the principle of least privilege.",
      "recommendation": "Users should regularly review and consolidate their Personal Access Tokens by navigating to User Settings > Personal Access Tokens in Azure DevOps. Revoke any unused or duplicate tokens, and ensure that each active token serves a specific purpose with the minimum required scopes.",
      "totalCounts": 7
    };

const pageTitle = computed(() => {
  return categoryLabels.get(scope) + '- Rule: ' + data.name;
})
</script>

<template>
  <SectionsHero class="px-10 pt-14 pb-18 grid grid-cols-1 gap-8 2xl:grid-cols-2">
    <div class="px-6 pt-8 2xl:pt-20 text-center">
        <UIcon name="i-lucide-shield-check" class="text-7xl text-complementary-600" />
      <h1 class="text-4xl text-center font-bold text-highlighted pb-4">Secure</h1>
      <p class="text-center font-medium text-pretty text-toned text-base/8 md:text-lg/8 mx-auto">
        Once the report has been generated, we won't leave you down!<br >
        Get deeper insights about our rules as each rule has a clear description and recommendations of valuable actions.
      </p>
    </div>
    <UCard class="2xl:max-w-[calc(50%)] 2xl:mx-auto shadow md:shadow-md/20 lg:shadow-xl/20 2xl:shadow-2xl/20" :ui="{body: 'flex flex-col gap-4'}">
        <LayoutPageHeader :title="pageTitle" severity="warning" :show-status-text="true" />
        <h3 class="text-left">Details</h3>
        <UCard>
          <div class="flex flex-col gap-4">
            <div class="line-height-1">
              <Label class="font-semibold">Description</Label>
              <p class="text-sm">{{ data.description }}</p>
            </div>
            <div>
              <Label class="font-semibold">Recommendation</Label>
              <p class="text-sm">{{ data.recommendation }}</p>
            </div>
          </div>
        </UCard>
    </UCard>
  </SectionsHero>
</template>

<style scoped></style>