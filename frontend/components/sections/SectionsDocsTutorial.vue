<script setup lang="ts">
defineProps<{
    title: string;
    src?: string;
    alt?: string;
}>();
</script>

<template>
    <h3 class="mt-4">{{ title }}</h3>

    <slot />

    <img class="pat-tutorial-img" :src="src" :alt="alt" />
</template>

<style scoped></style>