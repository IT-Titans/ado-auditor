<script setup lang="ts">
defineProps<{
  statusText?: string;
  stats: Array<Stat>;
}>();

const linkComponent = resolveComponent("ULink");

const baseClass = "flex items-center gap-2";
</script>

<template>
  <div class="flex flex-col w-full gap-4">
    <p v-if="statusText">
      Status: <span>{{ statusText }}</span>
    </p>
    <div class="flex gap-3 flex-col ml-2">
      <component
        v-for="stat in stats"
        :key="stat.label"
        :is="stat.to ? linkComponent : 'div'"
        v-bind="
          stat.to ? { to: stat.to, class: stat.linkClass || baseClass } : {}
        "
        :class="[baseClass, stat.class]"
      >
        <StatCircle :color="stat.color">
          <span class="font-semibold">{{ stat.value }}</span>
        </StatCircle>
        <span :class="[stat.to && 'hover:underline']">{{ stat.label }}</span>
      </component>
    </div>
  </div>
</template>
