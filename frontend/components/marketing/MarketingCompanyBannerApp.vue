<script setup lang="ts"></script>

<template>
  <UContainer
    class="bg-white h-16 flex justify-between items-center py-2 gap-6"
  >
    <img
      src="https://it-titans-gmbh.com/wp-content/uploads/2023/02/IT-Titans_Logo-1-300x82.png"
      alt="IT Titans GmbH Logo"
      class="object-contain w-38"
    />
    <div class="shrink">
      <h4 className="font-semibold text-sm">Made with ❤️ by IT Titans GmbH</h4>
      <p className="text-gray-700 text-sm">
        We build innovative solutions for your needs, with a passion for
        software development.
      </p>
    </div>
    <ULink target="_blank" to="https://it-titans-gmbh.com">
      Learn more about us
    </ULink>
  </UContainer>
</template>

<style scoped></style>
