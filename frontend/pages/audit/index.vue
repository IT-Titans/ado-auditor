<script setup lang="ts">
const expiredAfter = useRuntimeConfig().public.auditExpireTime;

const formattedExpiredAfter = computed(() =>
  formatDuration(Number.parseInt(expiredAfter) as number)
);
</script>

<template>
  <UContainer class="flex flex-col gap-24">
    <section>
      <HeadingsPageTitel>
        Audit your Organization <span class="text-base-900">for free</span>
      </HeadingsPageTitel>
      <TextsPageIntro>
        Get started to fully audit your Azure DevOps organizations security.
      </TextsPageIntro>
      <div class="flex flex-col gap-4">
        <UAlert
            v-if="$route.query.expired"
            color="info"
            icon="i-lucide-info"
            variant="soft"
            title="Audit expired"
            :description="`We store your audit in memory for ${formattedExpiredAfter}. If you want to view the audit again, you must restart it.`"
            :ui="{
                icon: 'size-5!',
                title: 'font-semibold',
              }"
        />
        <FormStartAudit />
      </div>
    </section>
    <AuditFaq />
    <section>
      <MarketingCompanyBanner />
    </section>
  </UContainer>
</template>

<style scoped></style>
