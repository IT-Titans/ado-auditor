<script setup lang="ts">
import * as v from "valibot";
import type { FormSubmitEvent } from "@nuxt/ui";

const props = defineProps<{
  auditId: string;
}>();

const emit = defineEmits<{
  (e: "close"): void;
}>();

const toast = useToast();
const sendIdeaFormSchema = v.object({
  acceptedDataPolicy: v.pipe(
    v.boolean(),
    v.literal(true, "You must accept the data policy.")
  ),
  name: v.nullable(
    v.pipe(v.string(), v.maxLength(200, "The name can have max 200 characters"))
  ),
  email: v.nullable(
    v.pipe(
      v.string(),
      v.maxLength(
        300,
        "Your email has a wrong format. Please add a valid email address."
      ),
      v.email("Please enter a valid email address")
    )
  ),
  feedback: v.pipe(
    v.string(),
    v.nonEmpty("Please enter your feedback"),
    v.maxLength(3000, "Feedback can have max 3000 characters")
  ),
});

type SendIdeaFormSchema = v.InferOutput<typeof sendIdeaFormSchema>;

const state = reactive({
  acceptedDataPolicy: false,
  name: null,
  email: null,
  feedback: "",
});

async function onSubmit(event: FormSubmitEvent<SendIdeaFormSchema>) {
  await $fetch("/api/feedback", {
    method: "POST",
    body: {
      acceptedDataPolicy: event.data.acceptedDataPolicy,
      name: event.data.name,
      email: event.data.email,
      feedback: event.data.feedback,
    },
    query: {
      auditId: props.auditId,
    },
    onResponse: () => {
      toast.add({
        title: "Successfully submit idea",
        description: "Thank you for submitting your idea for ADO Auditor!",
        color: "info",
      });
      emit("close");
    },
    onResponseError: ({ response }) => {
      console.error("Error sending idea:", response);
      toast.add({
        title: "Error",
        description: "There was an error sending your idea. Please try again.",
        color: "error",
      });
    },
  });
}
</script>

<template>
  <UForm
    :schema="sendIdeaFormSchema"
    :state="state"
    class="flex flex-col gap-4 w-full"
    @submit="onSubmit"
  >
    <UFormField label="Name" name="name" class="w-full text-md">
      <UInput
        v-model="state.name"
        placeholder="Your name (optional)"
        class="w-full"
      />
    </UFormField>
    <UFormField label="Email Address" name="email" class="w-full text-md">
      <UInput
        v-model="state.email"
        placeholder="Your email-address (optional)"
        class="w-full"
      />
    </UFormField>
    <UFormField
      label="Feedback"
      name="feedback"
      required
      class="w-full text-md"
    >
      <UTextarea
        v-model="state.feedback"
        placeholder="We're always looking for ways to make our service better. If you have a suggestion, feature request, or any creative idea, please share it with us! Your feedback helps us grow and improve."
        class="w-full"
      />
    </UFormField>
    <UFormField name="acceptedDataPolicy" class="w-full text-md">
      <UCheckbox
        v-model="state.acceptedDataPolicy"
        size="md"
        name="acceptedDataPolicy"
      >
        <template #label>
          <p>
            I accept the
            <ULink target="_blank" to="/privacy-policy" class="text-blue-500"
              >data policy</ULink
            >
            <span class="text-red-500"> *</span>
          </p>
        </template>
      </UCheckbox>
    </UFormField>
    <div class="flex gap-4 justify-end">
      <UButton variant="soft" color="neutral" @click="$emit('close')">
        Cancel
      </UButton>
      <UButton loading-auto type="submit"> Submit </UButton>
    </div>
  </UForm>
</template>

<style scoped></style>
