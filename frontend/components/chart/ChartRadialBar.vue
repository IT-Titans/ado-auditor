<script setup lang="ts">
import { computed } from "vue";
import type { Ring } from "~/types/types";

/** Props *************************************************************/
const props = defineProps<{
  rings: Ring[];
  /** Pixel diameter of the whole chart (default = 160) */
  size?: number;
  /** Width of each stroke (default = 10) */
  strokeWidth?: number;
  /** Gap between rings, in px (default = 4) */
  gap?: number;
  /** Text/number shown in the middle (slot overrides it) */
  display?: string | number;
  /** Extra Tailwind classes for the wrapper */
  wrapperClass?: string;
}>();

/** Internals *********************************************************/
const size = props.size ?? 160;
const strokeWidth = props.strokeWidth ?? 10;
const gap = props.gap ?? 4;
const center = size / 2;

/** Calculate geometry for every ring */
const processedRings = computed(() =>
  props.rings.map((ring, idx) => {
    const radius = center - strokeWidth / 2 - idx * (strokeWidth + gap);
    const circumference = 2 * Math.PI * radius;
    const dashOffset = circumference * (1 - ring.percent);
    return {
      ...ring,
      radius,
      circumference,
      dashOffset,
      trackClass: ring.trackClass ?? "stroke-stone-200", // Tailwind stone-200 fallback
    };
  })
);
</script>

<template>
  <!-- wrapper keeps the SVG and the centre label stacked -->
  <div
    :class="['relative', wrapperClass]"
    :style="{ width: size + 'px', height: size + 'px' }"
  >
    <svg :width="size" :height="size" :viewBox="`0 0 ${size} ${size}`">
      <!-- one <g> per ring -->
      <g
        v-for="(ring, i) in processedRings"
        :key="i"
        :transform="`rotate(-90 ${center} ${center})`"
      >
        <!-- background track -->
        <circle
          :r="ring.radius"
          :cx="center"
          :cy="center"
          fill="transparent"
          :class="[ring.trackClass]"
          :stroke-width="strokeWidth"
        />

        <!-- value arc -->
        <circle
          :r="ring.radius"
          :cx="center"
          :cy="center"
          fill="transparent"
          :class="[ring.strokeClass]"
          :stroke-width="strokeWidth"
          stroke-linecap="round"
          :stroke-dasharray="ring.circumference"
          :stroke-dashoffset="ring.dashOffset"
        />
      </g>
    </svg>

    <!-- centre label (you can slot in your own markup) -->
    <div class="absolute inset-0 flex items-center justify-center">
      <slot name="center">
        <span class="text-xl font-semibold">{{ display }}</span>
      </slot>
    </div>
  </div>
</template>

<style scoped>
/* prevents the stroke clipping on the SVG edges */
svg {
  overflow: visible;
}
</style>
