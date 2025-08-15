<script setup lang="ts">
import type { TableColumn } from "@nuxt/ui";

const props = defineProps<{
  data: BaseRule[];
  loading?: boolean;
  scope: Scope;
}>();

const route = useRoute();

const ULink = resolveComponent("ULink");
const TableCellSeverityIcon = resolveComponent("TableCellSeverityIcon");

const columns: TableColumn<BaseRule>[] = [
  {
    accessorKey: "name",
    header: "Rule",
    cell: ({ row }) => {
      const rowData = row.original;
      return h("div", { class: "flex flex-col text-wrap gap-2" }, [
        h(
          ULink,
          {
            class: "font-semibold",
            to: linkTemplates.audit.rules.rule(
              route.params.id as string,
              props.scope,
              rowData.id
            ),
          },
          () => rowData.name
        ),
        h(
          "span",
          { class: "border-l ml-2 pl-2 text-gray-500" },
          rowData.description
        ),
      ]);
    },
  },
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
];
</script>

<template>
  <UTable :data :columns :loading class="border rounded-md bg-white" />
</template>
