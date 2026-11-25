<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'
import type { ColumnDef } from '@tanstack/vue-table';
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
      active: true,
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
]);

type PatScopeEndpointLink = {
  label: string;
  url: string;
};

type PatScopeEntry = {
  scope: string;
  access: string;
  description: string;
  endpoints: PatScopeEndpointLink[];
};

const data: PatScopeEntry[] = [
  {
    scope: "Graph",
    access: "Read",
    description: "Read, group, scope, and add",
    endpoints: [
      { label: "Users - List", url: "https://learn.microsoft.com/en-us/rest/api/azure/devops/graph/users/list" },
      { label: "Groups - List", url: "https://learn.microsoft.com/en-us/rest/api/azure/devops/graph/groups/list" },
      { label: "Memberships - Get", url: "https://learn.microsoft.com/en-us/rest/api/azure/devops/graph/memberships/get" },
    ]
  },
  {
    scope: "Member Entitlement Management",
    access: "Read",
    description: "Read and manage users",
    endpoints: [
      { label: "User Entitlements - Get", url: "https://learn.microsoft.com/en-us/rest/api/azure/devops/memberentitlementmanagement/user-entitlements/get" },
    ]
  },
  {
    scope: "Project and Team",
    access: "Read",
    description: "Create, read, update, and delete",
    endpoints: [
      { label: "Projects - List", url: "https://learn.microsoft.com/en-us/rest/api/azure/devops/core/projects/list" },
    ]
  },
  {
    scope: "User Profile",
    access: "Read",
    description: "Write to profile",
    endpoints: [
      { label: "Projects - List", url: "https://learn.microsoft.com/en-us/rest/api/azure/devops/core/projects/list" },
    ]
  },
  {
    scope: "Token Administration",
    access: "Read & Manage",
    description: "Read and revoke",
    endpoints: [
      { label: "Personal Access Tokens - List", url: "https://learn.microsoft.com/en-us/rest/api/azure/devops/tokenadmin/personal-access-tokens/list" },
    ]
  },
];

const uLink = resolveComponent("ULink");

const columns = computed<ColumnDef<PatScopeEntry>[]>(() => [
  { accessorKey: "scope", header: "Scope", meta: { class: { th: 'font-bold', td: 'font-bold  text-black', }, } },
  { accessorKey: "access", header: "Access", meta: { class: { th: 'font-bold', td: 'font-bold text-black', }, } },
  {
    accessorKey: "endpoints", header: "Endpoints", cell: ({ cell }) => {
      const value = cell.getValue() as PatScopeEndpointLink[];
      return h('div', { class: 'flex flex-col', }, value.map(link => h(uLink, { to: link.url, target: '_blank' }, () => link.label)));
    }
  },
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
          PAT Generation
        </TextsPageIntro>
      </header>

      <div class="flex flex-col gap-4 pb-20 md:flex-row md:gap-8">
        <aside>
          <UNavigationMenu orientation="vertical" :items="items" class="md:data-[orientation=vertical]:w-48" />
        </aside>

        <div class="flex flex-col gap-8">
          <article class="prose max-w-none">
            <h2>What is a PAT?</h2>
            <p>
              PAT stands for Personal Access Token and is your key to the Azure DevOps APIs.
            </p>
          </article>

          <article class="prose max-w-none">
            <h2>Why do I need it?</h2>
            <p>
              As we use official Azure DevOps APIs to feed our rule engine with data, we need the PAT to be able to
              access the organizations
              data on your behalf.
            </p>
          </article>

          <article class="prose prose-img:border-1 max-w-none">
            <h2>How do I get a PAT?</h2>
            <p>
              Just follow these easy steps and you will get a PAT that can be used for your free audit in a couple of
              seconds.
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

            <h3>Create a New Personal Access Token (PAT)</h3>
            <p>
              3. On the Personal access tokens tab, click "New Token".
            </p>
            <img src="/assets/images/docs/pats/pats-step-3.png" alt="Shows how to create a new PAT" />

            <h3>Configure the PAT Details</h3>
            <p>
              4. Configure the PAT details as needed:<br />
              <strong>Name</strong>: Give your token a meaningful name, e.g., "PAT for ADO Auditor".<br />
              <strong>Organization</strong>: Select your target organization (the organization you want to audit).<br />
              <strong>Expiration</strong>: Choose an appropriate lifespan. We recommend to keep it as short as possible,
              e.g., just for the time of the audit.<br />
            </p>
            <p>
              5. Choose to configure the PAT scopes:<br />
              <strong>Scopes</strong>: Select "Custom defined".
            </p>
            <p>
              6. Click on "Show all scopes"
            </p>
            <img src="/assets/images/docs/pats/pats-step-4-6.png" alt="Shows how to customize PAT details" />

            <h3 id="set-custom-scopes" class="scroll-mt-24">Set Custom Scopes</h3>
            <p>
              7. Choose the required scopes to audit your organization<br />
              You will need to set several scopes as shown in the table below:
            </p>

            <UTable :columns="columns" :data="data" class="bg-white border rounded-md not-prose" />

            <p>An example to set the "Graph" scope with "Read" access:</p>
            <img src="/assets/images/docs/pats/pats-step-7.png" alt="Shows how to set the Graph:Read PAT scope" />
            <p>
              Note that we keep the scopes required to audit your organization at a minimum.<br />
              Each scope we request is mandatory and the audit will fail when the PAT is missing these scopes.
            </p>

            <h3>Create the PAT</h3>
            <p>
              Review your selected scopes. Make sure that you provide at minimum the exact scopes we listed above.<br />
              Additional scopes won't affect the audit but it's best practice to provide only the strict necessary
              scopes.
            </p>
            <p>
              8. Once done, click "Create".
            </p>
            <img src="/assets/images/docs/pats/pats-step-8.png" alt="Shows how to finally create the configured PAT" />

            <h3>Copy the PAT</h3>
            <p>
              9. Copy the token shown immediately after creation.
            </p>
            <p>
              You won't be able to see it again. If you miss copying the PAT, you need to create a new PAT. Be sure to
              delete the old PAT in this case.<br />
            </p>
            <p>
              Store the PAT securely (e.g., in a password manager or secret vault).
            </p>
            <img src="/assets/images/docs/pats/pats-step-9.png" alt="Shows how to copy the created PAT" />

            <h3>Notes</h3>
            <p>
              PATs are used for authenticating via scripts, REST APIs, and tools that don't support Azure AD
              authentication.<br />
              Always follow least privilege principles and set expiration dates thoughtfully.
            </p>
            <p>
              You can revoke or regenerate tokens at any time from the same security settings page.<br />
              We've created a step-by-step guide to assist you with this: <ULink to="/docs/pat-revocation">Revoke a PAT</ULink>.
            </p>
          </article>
        </div>
      </div>
    </article>
  </UContainer>
</template>

<style scoped></style>
