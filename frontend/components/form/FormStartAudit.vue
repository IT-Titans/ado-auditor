<script setup lang="ts">
import type { FormSubmitEvent } from "@nuxt/ui";
import * as v from "valibot";
import type {NuxtError} from "#app";

const loginSchema = v.object({
  organization: v.pipe(v.string(), v.nonEmpty("Organization Name is required.")),
  pat: v.pipe(v.string(), v.nonEmpty("Personal Access Token is required.")),
  email: v.pipe(v.string(), v.email("Email Address has a wrong format."),
      v.maxLength(
          300,
          "Your email has a wrong format. Please add a valid email address."
      ),
      v.nonEmpty("Email Address is required.")),
  acceptedDataPolicy: v.pipe(
    v.boolean(),
    v.literal(true, "You have to accept the data policy to make an audit.")
  ),
});

type LoginSchema = v.InferOutput<typeof loginSchema>;

const state = reactive({
  organization: "",
  pat: "",
  email: "",
  acceptedDataPolicy: false,
  acceptedAnonymousAuditLogging: false,
});

const loading = ref(false);
const isError = ref(false);
const error = ref<NuxtError | undefined>(undefined);

async function onSubmit(event: FormSubmitEvent<LoginSchema>) {
  loading.value = true;
  isError.value = false;

  try {
    const res = await $fetch("/api/audit", {
      method: "POST",
      body: JSON.stringify(event.data),
      headers: {
        "Content-Type": "application/json",
      },
      onResponseError: ({ response }) => {
        if (response.status === 401) {
          throw createError({
            statusCode: 401,
            statusMessage: "Invalid Organization Name or PAT."
          })
        } else if (response.status === 500) {
          throw createError({
            statusCode: response.status,
            statusMessage: getErrorMessage(500),
            data: response._data,
          });
        } else {
          throw createError({
            statusCode: response.status,
            statusMessage: "An unknown error has occurred.",
            data: response._data,
          });
        }
    },
    });

    navigateTo(`/audit/${res.data.id}`);
  } catch (err) {
    isError.value = true;
    loading.value = false;
    if (isNuxtError(err)) {
      error.value = err;
    }
    console.error("Error during form submission:", error);
    return;
  }
}
</script>

<template>
  <div class="flex flex-col items-center gap-4 w-full">
    <UAlert v-if="isError && error" icon="tabler:alert-octagon" color="error" variant="soft" title="Something went wrong!">
      <template #description>
        <span v-if="error.statusCode === 401">
          {{ error.statusMessage }} For more information how to generate an PAT, go to
          <ULink to="/docs/pat-generation" class="text-inherit font-bold">PAT Generation documentation</ULink>.
        </span>
        <span v-else>
          {{error.statusMessage}}
        </span>
      </template>
    </UAlert>
    <UForm :schema="loginSchema" :state class="grid grid-cols-1 md:grid-cols-2 gap-6 w-5/6"  @submit="onSubmit">
      <UFormField name="organization" required size="xl">
        <template #label>
          Organization Name
          <UTooltip text="The organization name is the unique identifier for your Azure DevOps Instance">
            <UIcon name="i-lucide-circle-question-mark" class="size-6" />
          </UTooltip>
        </template>
        <UInput v-model="state.organization" placeholder="Contoso" class="w-full"/>
      </UFormField>
      <UFormField name="pat" required size="xl">
        <template #label>
          Personal Access Token
          <UTooltip text="The Personal Access Token (PAT) is a secure way to authenticate and authorize access to your Azure DevOps resources.">
            <UIcon name="i-lucide-circle-question-mark" class="size-6" />
          </UTooltip>
        </template>
        <template #hint>
          <ULink to="/docs/pat-generation">Need Help?</ULink>
        </template>
        <UInput v-model="state.pat" type="password" placeholder="45faksjd59d65as6yd5cs5g9s" class="w-full" />
      </UFormField>
      <UFormField name="email" required size="xl" class="w-full md:col-span-2">
        <template #label>
          Email Address
        </template>
        <UInput v-model="state.email" placeholder="info@contoso.com" class="w-full" />
      </UFormField>
      <UFormField name="acceptedDataPolicy" size="xl" class="md:col-span-2">
        <UCheckbox v-model="state.acceptedDataPolicy" size="xl" name="acceptedDataPolicy">
          <template #label>
            I accept the
            <ULink target="_blank" href="/privacy-policy" class="text-blue-500" >data policy</ULink>
            <span class="text-red-500"> *</span>
          </template>
        </UCheckbox>
      </UFormField>
      <UFormField name="acceptedAnonymousAuditLogging" size="xl" class="md:col-span-2">
        <UCheckbox v-model="state.acceptedAnonymousAuditLogging" size="xl" name="acceptedAnonymousAuditLogging">
          <template #label>
            <p>
              Allow
              <UModal
                title="Anonymous data collection"
                :close="{
                  color: 'primary',
                  variant: 'ghost',
                }"
              >
                <span
                    class="focus-visible:outline-primary hover:text-default cursor-pointer transition-colors text-blue-500"
                    @click.prevent>
                  anonymous data collection
                </span>
                <template #body>
                  <LegalsAnonymousLogging />
                </template>
              </UModal>
            </p>
          </template>
        </UCheckbox>
      </UFormField>
      <UButton class="w-full py-3 px-6 md:col-span-2 text-center inline-block" type="submit" label="Submit" :disabled="loading" :loading="loading" />
      <div v-if="loading" class="fixed inset-0 bg-black/50 z-50 flex items-center justify-center">
        <div class="bg-white p-8 rounded shadow-md flex flex-col items-center">
          <UIcon name="tabler:loader-2" class="animate-spin mb-4" />
          <p>Creating your organization audit.</p>
          <p>Please wait while we create your audit. This may take a few minutes.</p>
          <p>As soon as the audit has been successfully created, you will be automatically forwarded to it.</p>
        </div>
      </div>
    </UForm>
  </div>
</template>

<style scoped></style>
