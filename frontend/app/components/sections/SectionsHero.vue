<script setup lang="ts">
import {cn} from "~/lib/utils";
import type {HTMLAttributes} from "vue";

const props = defineProps<{class?: HTMLAttributes["class"]}>();
</script>

<template>
  <section :class="cn('w-dvw max-w-full overflow-hidden', props.class)">
    <slot />
  </section>
</template>

<style scoped></style>