<script setup lang="ts">
import {rand} from "@vueuse/core";
import dayjs from 'dayjs'

const patDummyData: Array<Pat<PatUser>> = [
  {
    id: "Dummy",
    name: "Linux_Laptop",
    scopes: ["app_token"],
    user: {
      userId: "1",
      userName: "John Doe",
    },
    createdAt: dayjs().subtract(rand(60, 390), 'days').toString(),
    expiresIn: dayjs().subtract(rand(60, 390), 'days').toString(),
    affectedPatRulesCounts: {
      totalCriticals: 9,
      totalWarnings: 2,
      totalOks: 24
    }
  },
  {
    id: "Dummy",
    name: "Deployment Token",
    "scopes": [
      "vso.build_execute",
      "vso.code_full",
      "vso.variablegroups_manage"
    ],
    user: {
      userId: "1",
      userName: "Jane Doe",
    },
    createdAt: dayjs().subtract(rand(60, 390), 'days').toString(),
    expiresIn: dayjs().subtract(rand(60, 390), 'days').toString(),
    affectedPatRulesCounts: {
      totalCriticals: 1,
      totalWarnings: 0,
      totalOks: 2
    }
  },
  {
    id: "Dummy",
    name: "Git: https://dev.azure.com/Contoso",
    "scopes": [
      "vso.code_write",
      "vso.packaging"
    ],
    user: {
      userId: "1",
      userName: "John Doe",
    },
    createdAt: dayjs().subtract(rand(60, 390), 'days').toString(),
    expiresIn: dayjs().subtract(rand(60, 390), 'days').toString(),
    affectedPatRulesCounts: {
      totalCriticals: 0,
      totalWarnings: 3,
      totalOks: 4
    }
  },
  {
    id: "Dummy",
    name: "Code-Full for Shell",
    "scopes": [
      "vso.code_full"
    ],
    user: {
      userId: "1",
      userName: "Jane Doe",
    },
    createdAt: dayjs().subtract(rand(60, 390), 'days').toString(),
    expiresIn: dayjs().subtract(rand(60, 390), 'days').toString(),
    affectedPatRulesCounts: {
      totalCriticals: 0,
      totalWarnings: 1,
      totalOks: 2
    }
  }
];
</script>

<template>
  <SectionsHero class="grid grid-cols-1 gap-8 2xl:grid-cols-2 justify-end">
      <div class="px-6 pt-8 2xl:pt-20 text-center">
        <UIcon name="i-lucide-lightbulb" class="text-7xl text-triadic1-400"/>
        <h1 class="text-4xl font-bold text-highlighted pb-4 leading-10">
          Light up the dark
        </h1>
        <p class="text-base/8 font-medium text-pretty text-toned md:text-lg/8 2xl:mx-auto">
          Over time, users, permissions, and configurations can drift from best practices, increasing your exposure to risk.<br >
          Regular security audits help you identify and fix these gaps before attackers can exploit them. <br >
          Our software makes this process easier and more reliable, giving you peace of mind and better control over your development environment.
        </p>
      </div>
      <div class="py-10 2xl:py-20 px-10 2xl:pr-20 2xl:pl-0">
        <PatsTable class="shadow md:shadow-md/20 lg:shadow-xl/20 2xl:shadow-2xl/20" :data=patDummyData :disable-link="true" />
      </div>
  </SectionsHero>
</template>

<style scoped>

</style>