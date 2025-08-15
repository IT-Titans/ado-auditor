<script setup lang="ts">
import type { NuxtError } from "#app";

defineProps<{
  error: NuxtError | null;
  pending?: boolean;
}>();
</script>

<template>
  <slot v-if="pending" name="loading" />
  <slot v-else-if="error" name="error" :error="error">
    <UContainer class="min-h-full flex flex-1 flex-col py-4 gap-4">
      <ErrorsDisplay :status-code="error.statusCode" />
    </UContainer>
  </slot>
  <slot v-else />
</template>
