<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'
const items = ref<NavigationMenuItem[][]>([
  [
    {
      label: 'Introduction',
      description: 'Fully styled and customizable components for Nuxt.',
      icon: 'i-lucide-house',
      active: true,
      to: '/docs',
      class: "text-lg"
    },
    {
      label: 'PAT Generation',
      description: 'Learn how to create a PAT ( Personal Access Token )',
      icon: 'i-lucide-key-round',
      to: '/docs/pat-generation',
      class: "text-lg"
    },
    {
      label: 'PAT Revocation',
      description: 'Learn how to revoke / delete a PAT ( Personal Access Token )',
      icon: 'i-lucide-trash',
      to: '/docs/pat-revocation',
      class: "text-lg"
    }
  ]
])
</script>

<template>
  <UContainer>
    <article>
      <header>
        <HeadingsPageTitel>
          Documentation
        </HeadingsPageTitel>
        <TextsPageIntro>
          Introduction
        </TextsPageIntro>
      </header>
      <div class="flex flex-col gap-4 pb-20 md:flex-row md:gap-8">
        <aside>
          <UNavigationMenu orientation="vertical" :items="items" class="md:data-[orientation=vertical]:w-48" />
        </aside>

        <div class="flex flex-col gap-8">
          <article class="prose max-w-none">
            <h2>What is ADO Auditor?</h2>
            <p>
              ADO Auditor is an audit tool that helps you to get a clear view on your Azure DevOps security.<br />
              It does so by using a flexible rule engine that is capable to find hidden secrets and present them in an
              easy understandable way.
            </p>
            <p>
              It is not only useful when you need to manage hundreds of Azure DevOps users but comes also handy to find your hidden secrets fast.
            </p>
            <p>
              With the ability to export the audit you can keep track of your successes in securing your organization over time.
            </p>
          </article>

          <article class="prose max-w-none">
            <h2>Why choose ADO Auditor?</h2>

            <p>
              As we work a lot with Azure DevOps at <ULink to="https://it-titans-gmbh.de" target="_blank">IT-Titans</ULink> we searched for a way to keep an eye on security best practices.<br />
            </p>
            <p>
              We provide a comprehensive audit of your Azure DevOps organization, identifying potential security risks and providing actionable recommendations to mitigate them.
            </p>
          </article>
        </div>
      </div>
    </article>
  </UContainer>
</template>

<style scoped></style>
