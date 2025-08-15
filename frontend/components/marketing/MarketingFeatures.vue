<script setup lang="ts">
import Insights from "assets/images/showcase/insights.png";
import ShowAdmins from "assets/images/showcase/show-admins.png";
import Reports from "assets/images/showcase/reports-preview.png";
</script>

<template>
  <section class="py-12 sm:py-16 lg:py-20">
    <div class="px-4 mx-auto max-w-7xl sm:px-6 lg:px-8">
      <div class="text-center">
        <h2 class="text-3xl font-bold leading-tight text-gray-900 sm:text-4xl xl:text-5xl">Features with real benefits</h2>
      </div>

      <div class="grid grid-cols-1 mt-10 text-center sm:mt-16 sm:grid-cols-2 sm:gap-x-12 gap-y-12 md:grid-cols-3 md:gap-0 xl:mt-24">
        <div class="md:p-8 lg:p-14">
          <img :src="Insights" alt="Image showing different alerts" >
          <h3 class="mt-12 text-xl font-bold text-gray-900">Insights</h3>
          <p class="mt-5 text-base text-gray-600 font-pj">Get clear Security Insights that highlight risks, misconfigurations, and areas needing attention across your Azure DevOps organization.</p>
        </div>

        <div class="md:p-8 lg:p-14 md:border-l md:border-gray-200">
          <img :src="ShowAdmins" alt="Image showing different alerts" >
          <h3 class="mt-12 text-xl font-bold text-gray-900">Is your intern an admin?</h3>
          <p class="mt-5 text-base text-gray-600 font-pj">Quickly identify which of your users have admin access, helping you prevent unauthorized control over your resources.</p>
        </div>

        <div class="md:p-8 lg:p-14 md:border-l md:border-gray-200">
          <img :src="Reports" alt="You can send regular reports for your audits.">
          <h3 class="mt-12 text-xl font-bold text-gray-900">Reports</h3>
          <p class="mt-5 text-base text-gray-600 font-pj">Receive automatically generated reports on a regular schedule, giving you consistent visibility into your organization's security posture.</p>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped></style>