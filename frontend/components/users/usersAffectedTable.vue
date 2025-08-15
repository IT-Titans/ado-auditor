<script setup lang="ts">
import type { TableRow } from "@nuxt/ui";
import type { ColumnDef } from "@tanstack/vue-table";

defineProps<{
  data: Array<UserRuleItem>;
  columnVisibility?: Record<keyof UserRuleItem | string, boolean>;
}>();

const route = useRoute();

const CellStatCircleColumn = resolveComponent("TableCellStatCircleColumn");
const TableHeadersIcon = resolveComponent("TableHeadersIcon");

const columns = computed<ColumnDef<UserRuleItem>[]>(() => [
  {
    accessorKey: "userName",
    header: "User Name",
    meta: {
      class: {
        td: "text-highlighted",
      },
    },
  },
  {
    accessorKey: "metaType",
    header: "Account Type",
    meta: { class: { td: "capitalize" } },
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
]);

function onSelect(row: TableRow<UserRuleItem>) {
  useRouter().push(
    createLink("/audit/%s/users/%s", route.params.id as string, row.original.id)
  );
}
</script>

<template>
  <UTable
    :columns
    :data="data ?? []"
    class="bg-white border rounded-md"
    :meta="{ class: { tr: 'cursor-pointer' } }"
    @select="onSelect"
  />
</template>
