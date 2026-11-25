<script setup lang="ts"></script>

<template>
  <UContainer class="flex flex-1 flex-col gap-8">
    <div class="flex gap-4 w-full">
      <section class="flex flex-col gap-4 flex-1">
        <slot />
      </section>
    </div>
  </UContainer>
</template>

<style scoped></style>
