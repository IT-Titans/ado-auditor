<script setup lang="ts">
import type { TableRow } from "@nuxt/ui";
import type { ColumnDef } from "@tanstack/vue-table";

defineProps<{
  data: Array<BaseRule>;
}>();

const TableCellSeverityIcon = resolveComponent("TableCellSeverityIcon");
const CellStatCircleColumn = resolveComponent("TableCellStatCircleColumn");

const route = useRoute();

const columns = computed<ColumnDef<BaseRule>[]>(() => [
  { accessorKey: "id", header: "ID" },
  {
    accessorKey: "severity",
    header: "Severity",
    cell: ({ cell }) => {
      const severity = cell.getValue() as Severity;
      return h("div", { class: "flex items-center gap-2" }, [
        h(TableCellSeverityIcon, {
          severity,
        }),
        h("span", { class: "capitalize" }, severity),
      ]);
    },
  },
  {
    accessorKey: "name",
    header: "Rule",
    meta: { class: { td: "text-highlighted" } },
  },

  {
    accessorKey: "description",
    header: "Description",
    cell: ({ cell }) => {
      return h(
        "p",
        { class: "line-clamp-1 text-wrap" },
        cell.getValue() as string
      );
    },
  },
  {
    accessorKey: "totalCounts",
    header: "Affected Items",
    cell: ({ row }) => {
      const rule = row.original;

      return h(CellStatCircleColumn, {
        count: rule.totalCounts,
        severity: rule.severity,
      });
    },
    meta: {
      class: {
        th: "whitespace-nowrap",
      },
    },
  },
]);

function onSelect(row: TableRow<BaseRule>) {
  useRouter().push(
    linkTemplates.audit.rules.rule(
      route.params.id as string,
      row.original.scope,
      row.original.id
    )
  );
}
</script>

<template>
  <UTable
    :columns="columns"
    :data="data ?? []"
    :column-visibility="{ id: false }"
    class="bg-white border rounded-md"
    :meta="{ class: { tr: 'cursor-pointer' } }"
    @select="onSelect"
  />
</template>
