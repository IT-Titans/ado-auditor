<script setup lang="ts"></script>

<template>
  <SidebarProvider>
    <template #header>
      <header
        class="fixed z-10 bg-white top-0 flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12 border-b w-full"
      >
        <div class="flex items-center w-full gap-2 px-4 justify-between">
          <SidebarTrigger class="-ml-1" />
          <ModalLeaveAudit />
          <div>
            <ExportButton />
          </div>
        </div>
      </header>
    </template>
    <AppSidebar />
    <SidebarInset class="w-full min-h-full bg-gray-50">
      <NuxtPage />
      <template #footer>
        <MarketingCompanyBannerApp />
      </template>
    </SidebarInset>
  </SidebarProvider>
</template>
