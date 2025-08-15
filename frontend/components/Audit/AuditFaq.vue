<script setup lang="ts">
import type { AccordionItem } from '@nuxt/ui'

const config = useRuntimeConfig();
const formattedExpiredAfter = computed(() =>
    formatDuration(Number.parseInt(config.public.auditExpireTime) as number)
);
const faqSections = ref<AccordionItem[]>([
  {
    label: "What is my Organisation?",
    slot: 'organisation' as const,
  },
  {
    label: "What is a Personal Access Token?",
    slot: 'pat' as const,
  },
  {
    label: "How can I create a Personal Access Token to audit my organizations security?",
    slot: 'createPat' as const,
  },
  {
    label: "Can I trust your solution?",
    slot: 'trustUs' as const,
  },
]);
</script>

<template>
  <div>
    <div class="flex flex-col gap-6">
      <h2 class="text-center">FAQ</h2>
      <div class="w-20 h-1 bg-base-900 mx-auto" />
    </div>
    <UAccordion :items="faqSections" :ui="{trigger: 'font-normal', label: 'text-lg font-bold'}">
      <template #organisation>
        <p class="prose max-w-none mb-4">
          <strong>Organization</strong> is the name of your Azure DevOps organization.<br />
          You use it when you connect to the Azure DevOps services, e.g.: <strong>https://dev.azure.com/{YOUR-ORGANIZATION}</strong>
        </p>
      </template>
      <template #pat>
        <p class="prose max-w-none mb-4">A personal access token (PAT) is like a password that lets us securely access limited parts of your Azure DevOps Organization on your behalf.</p>
      </template>
      <template #createPat>
        <p class="prose max-w-none mb-4">That is done fairly easy. Just follow the description in our documentation: <ULink to="/docs/pat-generation" inactive-class="text-primary">PAT Generation</ULink></p>
      </template>
      <template #trustUs>
        <p class="prose max-w-none">We use your personal access token to securely access limited parts of your Azure DevOps organization using official API endpoints. We make it transparent for you to see which endpoints we use in our <ULink to="/docs/pat-generation#set-custom-scopes" inactive-class="text-primary">documentation</ULink></p>
        <p class="prose max-w-none">We do not store your personal access token, and we do not share it with anyone.</p>
        <p class="prose max-w-none">The only data we store is the compiled result of the audit. This information is stored in-memory for {{ formattedExpiredAfter }}. After that, all data are deleted.</p>
      </template>
    </UAccordion>
  </div>
</template>

<style scoped></style>
