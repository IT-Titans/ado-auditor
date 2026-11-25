<script setup lang="ts">
import { usedIcons } from "~/constants/icons";

definePageMeta({
  layout: "audit-default",
  title: "Users",
});

const router = useRouter();
const route = useRoute();
const auditId = route.params.id as string;

const { data, error } = await useAsyncData<UserResponse>("users", () =>
  $fetch<UserResponse>(`/api/users`, {
    method: "GET",
    query: {
      auditId,
    },
  })
);

if (error.value) {
  if (error.value.statusCode === 401 || error.value.statusCode === 410) {
    router.replace(linkTemplates.global.expiredAudit());
  }
}

const breadcrumbs = computed(() => [
  {
    icon: usedIcons.home,
    to: linkTemplates.audit.overview(auditId),
  },
  {
    label: "Users",
    icon: usedIcons.users.default,
    to: linkTemplates.audit.users.overview(auditId),
  },
]);

const pageTitle = computed(() => {
  return "Users";
});
</script>

<template>
  <DataFetchWrapper :error="error">
    <UBreadcrumb
      v-if="breadcrumbs"
      :items="breadcrumbs"
      class="flex items-center p-4 bg-white h-[50px] border-b"
    />
    <UContainer class="flex flex-1 flex-col py-4 gap-4 overflow-y-auto">
      <LayoutPageHeader :title="pageTitle" />
      <UsersTable :users="data?.users" />
    </UContainer>
  </DataFetchWrapper>
</template>

<style scoped></style>
