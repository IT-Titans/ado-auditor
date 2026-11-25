<script setup lang="ts">
import type { TableColumn, TableRow } from "@nuxt/ui";

defineProps<{
  users?: User[];
}>();

const router = useRouter();
const route = useRoute();

const UIcon = resolveComponent("UIcon");
const CellStatCircleColumn = resolveComponent("TableCellStatCircleColumn");
const TableHeadersIcon = resolveComponent("TableHeadersIcon");

const columns: TableColumn<User>[] = [
  {
    accessorKey: "userName",
    header: "Name",
    meta: {
      class: {
        td: "text-highlighted",
      },
    },
  },
  {
    accessorKey: "metaType",
    header: "Account Type",
    meta: { class: { td: "capitalize text-center", th: "text-center" } },
  },
  {
    accessorKey: "isActive",
    header: () => h("p", "Active"),
    cell: ({ row }) => {
      const icon = row.getValue("isActive")
        ? "i-tabler-user-check"
        : "i-tabler-user-x";

      return h("div", { class: "flex gap-2 items-center text-center" }, [
        h(UIcon, {
          name: icon,
          mode: "svg",
          class: "size-6",
        }),
        h("p", row.getValue("isActive") ? "Active" : "Inactive"),
      ]);
    },
    meta: {
      class: {
        th: "text-center",
        td: "flex justify-center",
      },
    },
  },
  {
    accessorKey: "affectedUserRulesCounts.totalCriticals",
    header: () =>
      h(TableHeadersIcon, {
        tooltipText: "Total Critical",
        icon: severityIcons.critical,
        iconColor: "text-severity-critical",
        iconClass: "size-5",
      }),
    cell: ({ cell }) => {
      return h(CellStatCircleColumn, {
        count: cell.getValue() ?? 0,
        severity: "critical",
      });
    },
    meta: {
      class: {
        th: "w-4",
        td: "w-4",
      },
    },
  },
  {
    accessorKey: "affectedUserRulesCounts.totalWarnings",
    header: () =>
      h(TableHeadersIcon, {
        tooltipText: "Total Warning",
        icon: severityIcons.warning,
        iconColor: "text-severity-warning",
        iconClass: "size-5",
      }),
    cell: ({ cell }) => {
      return h(CellStatCircleColumn, {
        count: cell.getValue() ?? 0,
        severity: "warning",
      });
    },
    meta: {
      class: {
        th: "w-4",
        td: "w-4",
      },
    },
  },
  {
    accessorKey: "affectedUserRulesCounts.totalOks",
    header: () =>
      h(TableHeadersIcon, {
        tooltipText: "Total Ok",
        icon: severityIcons.ok,
        iconColor: "text-severity-ok",
        iconClass: "size-5",
      }),
    cell: ({ cell }) => {
      return h(CellStatCircleColumn, {
        count: cell.getValue() ?? 0,
        severity: "ok",
      });
    },
    meta: {
      class: {
        th: "w-4",
        td: "w-4",
      },
    },
  },
];

const columnVisibility = ref({
  id: false,
});

function onSelect(_event: Event, row: TableRow<User>) {
  router.push(
    createLink("/audit/%s/users/%s", route.params.id as string, row.original.id)
  );
}
</script>

<template>
  <UTable
    :data="users"
    :columns
    :column-visibility="columnVisibility"
    class="border rounded-md bg-white"
    :meta="{ class: { tr: 'cursor-pointer' } }"
    @select="onSelect"
  />
</template>

<style scoped></style>
