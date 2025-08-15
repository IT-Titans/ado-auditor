<script setup lang="ts">

</script>

<template>
  <div id="pricing" class="py-20 md:px-20 lg:px-20 bg-gray-800">
    <div class="container mx-auto px-4">
      <div class="mb-12 text-center">
        <h3 class="text-white text-[30px] font-bold" data-aos="fade-down">
          Choose Your Perfect Package
        </h3>
        <p class="mx-auto mt-4 max-w-2xl text-gray-400" data-aos="fade-down">
          Select from our carefully curated packages designed to meet your specific needs and budget.
        </p>
      </div>

      <div class="grid gap-8 grid-cols-1 md:grid-cols-2">
        <div class="relative bg-[#1A1A1A] border-gray-800 p-6 rounded-xl hover:transform hover:-translate-y-2 transition-all duration-300">
          <div class="text-center">
            <h3 class="text-2xl font-bold text-white">Basic</h3>
            <div class="mt-4 text-center">
              <span class="text-4xl text-base-500 font-bold">Free</span>
            </div>
          </div>
          <div class="mt-6">
            <ul class="mb-8 space-y-4">
              <li class="flex items-center">
                <svg class="mr-2 h-5 w-5 text-base-500" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                  <path d="M9 12l2 2 4-4"></path>
                </svg>
                <span class="text-white">See token usages</span>
              </li>
              <li class="flex items-center">
                <svg class="mr-2 h-5 w-5 text-base-500" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true"><path d="M9 12l2 2 4-4"></path></svg>
                <span class="text-white">Get user insights</span>
              </li>
            </ul>
            <UButton :to="'/audit'" size="md" :label="'Start now'" />
          </div>
        </div>

        <div class="relative bg-base-950/20 backdrop-blur-lg rounded-2xl border-2 border-base-800 hover:transform hover:-translate-y-2 transition-all duration-300">
          <div class="absolute -top-4 left-1/2 -translate-x-1/2 rounded-full bg-base-800 px-3 py-1 text-white text-sm font-semibold">
            Most Popular
          </div>
          <div class="text-center p-6">
            <h3 class="text-2xl font-bold text-white">Premium</h3>
            <div class="mt-4 text-center">
              <span class="text-4xl text-base-500 font-bold">Upcoming</span>
            </div>
          </div>
          <div class="px-4 pb-8">
            <ul class="mb-8 space-y-4">
              <li class="flex items-center">
                <svg class="mr-2 h-5 w-5 text-base-500" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true"><path d="M9 12l2 2 4-4"></path></svg>
                <span class="text-white">All Features of Free package</span>
              </li>
              <li class="flex items-center">
                <svg class="mr-2 h-5 w-5 text-base-500" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true"><path d="M9 12l2 2 4-4"></path></svg>
                <span class="text-white">Add your own rules</span>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>

</style>