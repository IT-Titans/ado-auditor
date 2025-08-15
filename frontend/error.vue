<script setup lang="ts">
import type { NuxtError } from "#app";

defineProps({
  error: Object as () => NuxtError,
});
</script>

<template>
  <NuxtLayout>
    <UContainer class="flex justify-center flex-col items-center h-full">
      <div class="text-center">
        <p class="text-3xl text-base font-semibold text-primary">
          {{ error?.statusCode }}
        </p>
        <h2
          class="mt-4 text-5xl font-semibold tracking-tight text-balance sm:text-7xl"
        >
          Page not found
        </h2>
        <p class="mt-6 text-lg font-medium text-pretty sm:text-xl/8">
          Sorry, we couldn’t find the page you’re looking for.
        </p>
        <div class="mt-10 flex items-center justify-center gap-x-6">
          <ULink to="/">Go back home</ULink>
        </div>
      </div>
    </UContainer>
  </NuxtLayout>
</template>
