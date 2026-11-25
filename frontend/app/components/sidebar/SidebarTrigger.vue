<script setup lang="ts">
import { useSidebar } from ".";
import type { HTMLAttributes } from "vue";
import { PanelLeft } from "lucide-vue-next";
import { cn } from "@/lib/utils";

const props = defineProps<{
  class?: HTMLAttributes["class"];
}>();

const { toggleSidebar } = useSidebar();
</script>

<template>
  <UButton
    data-sidebar="trigger"
    data-slot="sidebar-trigger"
    variant="ghost"
    color="neutral"
    :class="cn('h-7 w-7 size-9', props.class)"
    @click="toggleSidebar"
  >
    <PanelLeft />
    <span class="sr-only">Toggle Sidebar</span>
  </UButton>
</template>
