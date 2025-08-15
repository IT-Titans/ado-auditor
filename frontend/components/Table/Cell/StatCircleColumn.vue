<script setup lang="ts">
defineProps<{
  count: number;
  severity: Severity;
}>();
</script>

<template>
  <div class="flex justify-center">
    <StatCircle :color="count > 0 ? severity : 'default'">
      {{ count }}
    </StatCircle>
  </div>
</template>
