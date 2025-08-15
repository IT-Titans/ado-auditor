<script setup lang="ts">
import { useSidebar } from "../sidebar/utils";
import { ref } from "vue";
import { VisuallyHidden } from "reka-ui";

const route = useRoute();
const open = ref(false);

const sidebarState = useSidebar();

function closeModal() {
  open.value = false;
}
</script>

<template>
  <UModal
    v-if="sidebarState.state.value !== 'collapsed'"
    v-model:open="open"
    :dismissible="false"
  >
    <template #header>
      <div class="flex justify-between items-center w-full">
        <div class="flex items-center gap-2">
          <UIcon
            name="i-tabler-heart"
            mode="svg"
            class="size-5 text-violet-500"
          />
          <span>We'd love to hear your ideas!</span>
        </div>
        <UButton variant="ghost" icon="i-tabler-x" @click="closeModal" />
      </div>
    </template>
    <template #description>
      <VisuallyHidden as-child>
        <span>
          We are always looking for ways to improve our service. If you have a
          suggestion, feature request, or any creative idea, please share it
          with us! Your feedback helps us grow and improve.
        </span>
      </VisuallyHidden>
    </template>
    <UButton
      variant="outline"
      color="neutral"
      class="bg-violet-100 h-fit flex justify-items-start w-full hover:bg-violet-400/30 cursor-pointer relative overflow-visible"
    >
      <div class="flex flex-col gap-2 items-start grow">
        <p class="text-lg">Ideas!?</p>
        <p class="font-light">Give us your feedback</p>
      </div>
      <UIcon
        name="i-lucide-rocket"
        mode="svg"
        class="size-10 absolute right-5 top-3 text-violet-800"
      />
    </UButton>
    <template #body>
      <div class="space-y-4">
        <FormSendFeedback
          :audit-id="(route.params.id as string)"
          @close="closeModal"
        />
      </div>
    </template>
  </UModal>
</template>

<style scoped></style>
