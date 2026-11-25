<script setup lang="ts">
</script>

<template>
  <section class="not-prose relative left-1/2 w-dvw max-w-full -translate-x-1/2 pb-20 xl:pb-40 overflow-hidden px-10 xl:px-0">
    <div class="absolute inset-x-0 -z-10 transform-gpu overflow-hidden blur-3xl" aria-hidden="true">
      <div
          class="relative left-[calc(50%-11rem)] aspect-1155/678 w-144.5 -translate-x-1/2 rotate-30 bg-linear-to-tr from-analogus2-600 to-primary-600 opacity-30 sm:left-[calc(50%-30rem)] sm:w-288.75"
          style="clip-path: polygon(74.1% 44.1%, 100% 61.6%, 97.5% 26.9%, 85.5% 0.1%, 80.7% 2%, 72.5% 32.5%, 60.2% 62.4%, 52.4% 68.1%, 47.5% 58.3%, 45.2% 34.5%, 27.5% 76.7%, 0.1% 64.9%, 17.9% 100%, 27.6% 76.8%, 76.1% 97.7%, 74.1% 44.1%)"/>
    </div>
    <div class="py-32">
      <div class="text-center">
        <HeadingsFullPage>Fast, free and fully private <br> Audits of your Azure DevOps setup</HeadingsFullPage>
       <TextsFullPageIntro>Valuable insights in seconds</TextsFullPageIntro>
        <div class="mt-10 flex items-center justify-center gap-x-9">
          <UButton :to="'/audit'" size="xl2" :label="'Start now'" />
          <a href="/docs" class="text-xl/6 font-semibold text-gray-900">Learn more <span aria-hidden="true">→</span></a>
        </div>
      </div>
    </div>
    <div class="max-w-(--ui-container) mx-auto grid grid-cols-1 lg:grid-cols-2 gap-8">
      <SummaryCard class="shadow md:shadow-md/20 lg:shadow-xl/20 2xl:shadow-2xl/20" :scope="'pats'" :total-count="6" :ok-count="3" :warning-count="2" :critical-count="1" :rule-links-inactive="true" />
      <SummaryCard class="shadow md:shadow-md/20 lg:shadow-xl/20 2xl:shadow-2xl/20" :scope="'users'" :total-count="23" :ok-count="14" :warning-count="9" :critical-count="0" :rule-links-inactive="true" />
    </div>
    <div class="absolute inset-x-0 top-[calc(100%-13rem)] -z-10 transform-gpu overflow-hidden blur-3xl sm:top-[calc(100%-30rem)]" aria-hidden="true">
      <div
          class="relative left-[calc(50%+3rem)] aspect-1155/678 w-144.5 -translate-x-1/2 bg-linear-to-tr from-primary-400 to-analogus2-500 opacity-30 sm:left-[calc(50%+36rem)] sm:w-288.75"
          style="clip-path: polygon(74.1% 44.1%, 100% 61.6%, 97.5% 26.9%, 85.5% 0.1%, 80.7% 2%, 72.5% 32.5%, 60.2% 62.4%, 52.4% 68.1%, 47.5% 58.3%, 45.2% 34.5%, 27.5% 76.7%, 0.1% 64.9%, 17.9% 100%, 27.6% 76.8%, 76.1% 97.7%, 74.1% 44.1%)" />
    </div>
  </section>
</template>

<style scoped></style>