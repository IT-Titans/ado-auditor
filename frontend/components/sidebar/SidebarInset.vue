<script setup lang="ts">
import type { HTMLAttributes } from "vue";
import { cn } from "@/lib/utils";

const props = defineProps<{
  class?: HTMLAttributes["class"];
}>();
</script>

<template>
  <main
    data-slot="sidebar-inset"
    :class="
      cn(
        'bg-background relative flex w-full flex-1 flex-col min-h-screen',
        'md:peer-data-[variant=inset]:m-2 md:peer-data-[variant=inset]:ml-0 md:peer-data-[variant=inset]:rounded-xl md:peer-data-[variant=inset]:shadow-sm md:peer-data-[variant=inset]:peer-data-[state=collapsed]:ml-2',
        props.class
      )
    "
  >
    <div class="flex flex-col flex-1 min-h-0">
      <div class="flex flex-col flex-1 overflow-y-auto mt-16">
        <slot />
      </div>
      <!-- TODO: Try to position the banner so that it remains at the bottom and does not disappear with more content in the main. -->
      <div
        v-if="$slots.footer"
        class="sticky bottom-0 w-full flex items-center border-t bg-white z-10"
      >
        <slot name="footer" />
      </div>
    </div>
  </main>
</template>
