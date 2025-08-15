<script setup lang="ts">
import type { NavigationMenuItem } from "@nuxt/ui";

const navItems = ref<NavigationMenuItem[]>([
  {
    label: "About us",
    to: "/about",
  },
  {
    label: "Documentation",
    to: "/docs",
  },
  {
    label: "Contact",
    to: "/contact",
  },
]);
</script>

<template>
  <div class="flex flex-col min-h-screen">
    <AppHeader :nav-items="navItems" />
    <main class="flex flex-col flex-1 bg-muted pt-32">
      <slot />
    </main>
    <AppFooter />
  </div>
</template>

<style scoped></style>
