<script setup lang="ts">
</script>

<template>
  <UContainer class="pt-8 pb-10 md:pb-20">
    <footer class="flex flex-col gap-4 py-4 md:flex-row md:justify-between md:gap-0">
      <aside class="flex flex-col gap-2 text-center md:text-left">
        <h6 class="text-lg">ADO Auditor</h6>
        <div class="flex items-center justify-center gap-2 text-xl md:justify-start md:items-start">
          <ULink to="https://www.linkedin.com/company/it-titans-gmbh" target="_blank">
            <UIcon name="i-tabler-brand-linkedin" />
          </ULink>
          <ULink to="https://github.com/it-titans" target="_blank">
            <UIcon name="i-tabler-brand-github" />
          </ULink>
        </div>
      </aside>
      <div class="flex gap-4 justify-between md:text-left md:gap-8">
        <nav class="flex flex-col gap-2">
          <h6 class="text-lg">Support</h6>
          <ULink to="/docs">Documentation</ULink>
        </nav>
        <nav class="flex flex-col gap-2">
          <h6 class="text-lg">Company</h6>
          <ULink to="/about">About us</ULink>
          <ULink to="/contact">Contact</ULink>
        </nav>
        <nav class="flex flex-col gap-2">
          <h6 class="text-lg">Legal</h6>
          <ULink to="/imprint">Imprint</ULink>
          <ULink to="/privacy-policy">Privacy policy</ULink>
        </nav>
      </div>
    </footer>
  </UContainer>
</template>

<style scoped></style>
