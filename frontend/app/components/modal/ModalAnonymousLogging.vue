<script setup lang="ts">
</script>

<template>
  <UModal
      title="Anonymous data collection"
      :ui="{overlay: 'fixed inset-0 bg-black/80', content: 'fixed divide-y divide-default flex flex-col focus:outline-none min-w-[calc(25%)]', header: 'flex items-center gap-1.5 p-4 sm:px-6 min-h-16 text-2xl', close: 'absolute top-3 end-4 text-2xl'}"
  >
    <span
        class="focus-visible:outline-primary hover:text-default cursor-pointer transition-colors text-blue-500"
        @click.prevent>
          anonymous data collection
    </span>
    <template #body>
      <LegalAnonymousLogging />
    </template>
  </UModal>
</template>

<style scoped></style>