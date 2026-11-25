<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'
const items = ref<NavigationMenuItem[][]>([
  [
    {
      label: 'Introduction',
      description: 'Fully styled and customizable components for Nuxt.',
      icon: 'i-lucide-house',
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
      active: true,
      to: '/docs/pat-revocation',
      class: "text-lg"
    }
  ]
]);
</script>

<template>
  <UContainer>
    <article>
      <header>
        <HeadingsPageTitel>
          Documentation
        </HeadingsPageTitel>
        <TextsPageIntro>
          PAT Revocation
        </TextsPageIntro>
      </header>

      <div class="flex flex-col gap-4 pb-20 md:gap-8 md:flex-row">
        <aside>
          <UNavigationMenu orientation="vertical" :items="items" class="md:data-[orientation=vertical]:w-48" />
        </aside>

        <div class="flex flex-col gap-8">
          <article class="prose prose-img:border-1 max-w-none">
            <h2>How do I revoke / delete a PAT?</h2>
            <p>
              Just follow these easy steps to revoke / delete an existing PAT.<br />
              Once the Personal Access Token (PAT) has been revoked, no one will be able to use it anymore.
            </p>

            <h3>Sign In to Azure DevOps</h3>
            <p>
              Go to: <ULink to="https://dev.azure.com/" target="_blank">https://dev.azure.com/</ULink> and sign in with
              your organization-connected Microsoft account.<br>
              Be sure to select the correct Azure DevOps organization your want to audit if prompted.
            </p>

            <h3>Open the "User settings"</h3>
            <p>
              1. In the top-right corner, click on <strong>"User settings"</strong> just next to your profile
              picture.
            </p>
            <p>
              2. Select <strong>"Personal access tokens"</strong> from the dropdown.
            </p>
            <img src="/assets/images/docs/pats/pats-step-1-2.png"
              alt="Shows how to start creating a PAT in Azure DevOps" />

            <h3>Choose the Personal Access Token (PAT) to revoke / delete</h3>
            <p>
              3. Select the PAT you want to revoke / delete by clicking on it.
            </p>
            <img src="/assets/images/docs/pats/revoke-pat-step-3.png" alt="Shows how to select an existing PAT to revoke" />

            <h3>Revoke the PAT</h3>
            <p>
              4. Make sure you've selected the correct PAT you want to revoke / delete.
            </p>
            <p>
              5. Click on "Revoke"<br />
              You will be asked to confirm this step.
            </p>
            <img src="/assets/images/docs/pats/revoke-pat-step-4-5.png" alt="Shows how to revoke an existing PAT" />

            <h3>Confirm PAT revocation</h3>
            <p>
              6. Confirm that you want to revoke / delete the selected PAT
            </p>
            <img src="/assets/images/docs/pats/revoke-pat-step-6.png" alt="Shows how to confirm PAT revocation" />
            <p>
              You successfully revoked the PAT. You can always create new PATs as shown in our guide: <ULink to="/docs/pat-generation">Generate a PAT</ULink>.
            </p>
          </article>
        </div>
      </div>
    </article>
  </UContainer>
</template>

<style scoped></style>
