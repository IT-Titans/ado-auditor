<script setup lang="ts"></script>

<template>
  <section class="flex flex-col md:flex-row items-center gap-6 py-10">
    <img
      src="https://it-titans-gmbh.com/wp-content/uploads/2023/02/IT-Titans_Logo-1-300x82.png"
      alt="IT Titans GmbH Logo"
      class="object-scale-down basis-1/2">
    <div class="basis-1/2 max-w-(--ui-container) text-center md:text-left">
      <h4 class="text-xl font-semibold mb-1">Made with ❤️ by IT Titans GmbH</h4>
      <p class="text-gray-700 mb-2">
        We build innovative solutions for your needs, with a passion for software development.
      </p>
      <ULink
        target="_blank"
        to="https://it-titans-gmbh.com"
        class="inline-block mt-2 px-4 py-2 bg-it-titans-blue rounded-md font-medium text-inverted hover:text-inverted hover:underline"
      >
        Learn more about us
      </ULink>
    </div>
  </section>
</template>

<style scoped>
  @keyframes animate-bob {
    0%,
    100% {
      transform: translateY(0);
    }
    50% {
      transform: translateY(-8px);
    }
  }

  .animation-bob:hover {
    animation: animate-bob 1.5s infinite;
    animation-timing-function: ease-in-out;
  }
</style>
