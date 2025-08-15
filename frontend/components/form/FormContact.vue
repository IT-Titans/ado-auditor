<script setup lang="ts">
import * as v from 'valibot'
import type { FormSubmitEvent } from '@nuxt/ui'

const toast = useToast()
const schema = v.object({
  name: v.nullable(
      v.pipe(v.string(), v.maxLength(200, "The name can have max 200 characters"))
  ),
  phone: v.nullable(
      v.pipe(v.string(), v.maxLength(200, "The phone number can have max 200 characters"))
  ),
  email: v.pipe(
      v.string(),
      v.maxLength(
          300,
          "Your email has a wrong format. Please add a valid email address."
      ),
      v.email("Please enter a valid email address")
  ),
  message: v.pipe(
      v.string(),
      v.nonEmpty('Please provide your message'),
      v.maxLength(
          2000,
          "Your message can have max 2000 characters"
      )
  ),
  acceptedDataPolicy: v.pipe(
      v.boolean(),
      v.literal(true, "You have to accept the data policy to send a message.")
  ),
})

type Schema = v.InferOutput<typeof schema>

const loading = ref(false);
const state = reactive({
  name: '',
  phone: '',
  company: '',
  email: '',
  message: '',
  acceptedDataPolicy: false
})

async function onSubmit(event: FormSubmitEvent<Schema>) {
  loading.value = true;

  try {
    await $fetch("/api/contact", {
      method: "POST",
      body: JSON.stringify(event.data),
      headers: {
        "Content-Type": "application/json",
      },
    });

    toast.add({ title: 'Success', description: 'The form has been submitted.', color: 'success' })
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
  } catch (err) {
    toast.add({ title: 'Error', description: 'There was a error submitting your message, please try again later!', color: 'error' })
  }
  loading.value = false;
}
</script>

<template>
  <UForm :schema="schema" :state="state" class="grid grid-cols-1 md:grid-cols-2 gap-6" @submit="onSubmit">
    <UFormField label="Name" name="name" size="xl">
      <UInput v-model="state.name" class="w-full" placeholder="John Doe" />
    </UFormField>
    <UFormField label="Mobile Number" name="phone" size="xl">
      <UInput v-model="state.phone" class="w-full" placeholder="+49 123 4567890" />
    </UFormField>
    <UFormField label="Company" name="company" size="xl" class="md:col-span-2 adjust" aria-autocomplete="none" autocapitalize="none">
      <UInput v-model="state.company" class="w-full" />
    </UFormField>
    <UFormField label="Email Address" name="email" size="xl" class="md:col-span-2" required>
      <UInput v-model="state.email" class="w-full" placeholder="john@example.com" />
    </UFormField>
    <UFormField label="Your Message" name="message" size="xl" class="md:col-span-2" required aria-autocomplete="none" autocapitalize="none">
      <UTextarea v-model="state.message" class="w-full" />
    </UFormField>
    <UFormField name="acceptedDataPolicy" class="w-full text-md">
      <UCheckbox v-model="state.acceptedDataPolicy" size="md" name="acceptedDataPolicy">
        <template #label>
          <p>
            I accept the
            <ULink target="_blank" href="/privacy-policy" class="text-blue-500" >data policy</ULink>
            <span class="text-red-500"> *</span>
          </p>
        </template>
      </UCheckbox>
    </UFormField>
    <UButton type="submit" class="w-full py-3 px-6 md:col-span-2 text-center inline-block" :disabled="loading" :loading="loading" size="xl2">Submit</UButton>
    <div v-if="loading" class="fixed inset-0 bg-black/50 z-50 flex items-center justify-center">
      <div class="bg-white p-8 rounded shadow-md flex flex-col items-center">
        <UIcon name="tabler:loader-2" class="animate-spin mb-4" />
        <p>Please wait while we send your message.</p>
      </div>
    </div>
  </UForm>
</template>

<style scoped>
.adjust {
  display: none;
}
</style>