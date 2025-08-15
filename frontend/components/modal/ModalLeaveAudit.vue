<script setup lang="ts">
import Logo from "~/assets/images/logo.png";
import { VisuallyHidden } from "reka-ui";
</script>

<template>
  <UModal
    title="Leave audit"
    :close="{
      color: 'primary',
      variant: 'ghost',
    }"
  >
    <UButton variant="link" class="cursor-pointer">
      <img :src="Logo" alt="ADO Auditor Logo" class="h-10" />
    </UButton>
    <template #description>
      <VisuallyHidden as-child>
        <span> You are leaving your current audit. Are you sure? </span>
      </VisuallyHidden>
    </template>
    <template #body="{ close }">
      <div class="flex flex-col gap-2">
        <p class="text-lg">You are leaving your current audit. Are you sure?</p>
        <p class="text-sm font-light">
          As long as the audit has not expired, you can return to it at any time
          via the browser history.
        </p>
        <div class="flex items-center justify-end gap-1.5">
          <UButton variant="soft" color="neutral" label="No" @click="close" />
          <UButton
            label="Yes"
            @click="$router.push(linkTemplates.global.home())"
          />
        </div>
      </div>
    </template>
  </UModal>
</template>
