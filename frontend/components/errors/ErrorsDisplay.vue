<script setup lang="ts">
const props = defineProps<{
  statusCode?: number;
}>();

const config = computed(
  () => errorMassages[props.statusCode ?? 500] || errorMassages[500]
);

const route = useRoute();
const router = useRouter();
function tryAgain() {
  router.push(route.fullPath);
}
</script>

<template>
  <div className="my-auto mx-auto text-center">
    <UCard :ui="{ body: 'flex flex-col' }">
      <UIcon
        v-if="config.icon"
        :name="config.icon"
        mode="svg"
        :class="[
          'flex self-center items-center justify-center mb-8',
          config.iconClass,
        ]"
      />
      <div class="mb-2">
        <span
          class="inline-block px-3 py-1 text-sm font-medium bg-slate-100 text-slate-600 rounded-full mb-4"
          >Error {{ statusCode }}
        </span>
      </div>

      <h1 class="text-3xl font-bold text-slate-900 mb-4">
        {{ config.title }}
      </h1>

      <p class="text-lg text-slate-600 mb-8 leading-relaxed">
        {{ config.message }}
      </p>

      <div v-if="config.suggestions" class="mb-8">
        <h3
          class="text-sm font-semibold text-slate-900 mb-4 uppercase tracking-wide"
        >
          Here's what you can try:
        </h3>
        <ul class="text-left space-y-2 max-w-md mx-auto">
          <li
            v-for="suggestion in config.suggestions"
            :key="suggestion"
            class="list-disc"
          >
            <span
              class="flex-shrink-0 w-1.5 h-1.5 bg-slate-400 rounded-full mt-2 mr-3"
            />
            <span class="text-slate-600">
              {{ suggestion }}
            </span>
          </li>
        </ul>
      </div>

      <div class="flex flex-col sm:flex-row gap-4 justify-center items-center">
        <ULink :to="'/audit/' + $route.params.id">Go to overview</ULink>
        <UButton @click="tryAgain">Try Again</UButton>
        <UButton @click="$router.back">Go Back</UButton>
      </div>
    </UCard>
    <div class="mt-16 text-center">
      <div class="max-w-md mx-auto">
        <h2 class="text-xl font-semibold text-slate-900 mb-4">
          Still need help?
        </h2>
        <p class="text-slate-600 mb-6">
          Our support team is here to help you get back on track.
        </p>
        <ULink to="/contact" variant="outline">Contact Support</ULink>
      </div>
    </div>
  </div>
</template>
