<script setup lang="ts">
import Logo from "~/assets/images/logo_trans.png";
import type { NavigationMenuItem } from "@nuxt/ui";

defineProps<{
  navItems: NavigationMenuItem[];
  ctaButtonText?: string;
}>();
</script>

<template>
  <header class="fixed top-0 w-full z-50 border-b py-4 glass-effect px-10 sm:px-6">
    <UContainer v-bind="$attrs" class="flex md:hidden justify-between items-center max-w-(--ui-container) ">
      <ULink to="/" class="sm:float-left inline-block">
        <img class="h-12 sm:h-10 mx-auto sm:mx-0" :src="Logo" alt="ADO Auditor - Get valuable insights to your Azure DevOps setup.">
      </ULink>
      <UDrawer direction="right" >
        <UButton icon="i-lucide-menu" size="xl"/>
        <template #content>
          <UContainer class="flex flex-col gap-4 min-w-[14rem] py-4">
            <UNavigationMenu :items="navItems" variant="link" orientation="vertical" :ui="{link: 'text-md font-normal'}" />
            <UButton :to="ctaButtonText ? '#' : '/audit'" size="md" :label="ctaButtonText ?? 'Start now'" />
          </UContainer>
        </template>
      </UDrawer>
    </UContainer>
    <UContainer v-bind="$attrs" class="hidden md:flex justify-between items-center sm:px-4 xl:px-0">
      <ULink to="/" class="sm:float-left inline-block">
        <img class="h-12 sm:h-10 mx-auto sm:mx-0" :src="Logo" alt="ADO Auditor - Get valuable insights to your Azure DevOps setup.">
      </ULink>
      <div class="flex items-center gap-4">
        <UNavigationMenu :items="navItems" variant="link" as="nav" :ui="{link: 'text-md font-normal'}"/>
        <UButton :to="ctaButtonText ? '#' : '/audit'" size="md" :label="ctaButtonText ?? 'Start now'" />
      </div>
    </UContainer>
  </header>
</template>

<style scoped>
  .glass-effect {
    backdrop-filter: blur(16px);
    background: rgba(255, 255, 255, 0.7);
  }
</style>