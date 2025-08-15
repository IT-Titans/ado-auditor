<script setup lang="ts">
import type { TableRow } from "@nuxt/ui";
import type { ColumnDef } from "@tanstack/vue-table";

const props = defineProps<{
  data: Array<Pat<PatUser>>;
  columnVisibility?: Record<keyof Pat<PatUser> | string, boolean>;
  disableLink?: boolean;
}>();

const UBadge = resolveComponent("UBadge");
const ULink = resolveComponent("ULink");
const CellStatCircleColumn = resolveComponent("TableCellStatCircleColumn");
const TableHeadersIcon = resolveComponent("TableHeadersIcon");

const route = useRoute();

const columns = computed<ColumnDef<Pat<PatUser>>[]>(() => [
  { accessorKey: "id", header: "ID" },
  {
    accessorKey: "name",
    header: "PAT-Name",
    cell: ({ cell }) => {
      const name = cell.getValue() as string;
      return h("p", { class: "text-wrap" }, name);
    },
    meta: {
      class: {
        td: "text-highlighted",
      },
    },
  },
  {
    accessorKey: "user.userName",
    header: "User",
    cell: ({ row, cell }) => {
      const userId = row.original.user?.userId as string;
      const userName = cell.getValue() as string;

      if (userName) {
        if (props.disableLink) {
          return userName;
        }

        return h(
          ULink,
          {
            to: createLink(
              "/audit/%s/users/%s",
              route.params.id as string,
              userId
            ),
          },
          () => userName
        );
      }
      return "No Data";
    },
  },
  {
    accessorKey: "scopes",
    header: "Scopes",
    cell: ({ cell }) => {
      const scopes = cell.getValue() as Array<string>;

      if (scopes.length > 0) {
        return h(
          "div",
          { class: "flex gap-2 flex-wrap max-w-sm overflow-hidden" },
          scopes.map((scope) =>
            h(UBadge, { variant: "outline", color: "neutral" }, () => scope)
          )
        );
      }
      return "No scopes defined.";
    },
  },
  {
    accessorKey: "createdAt",
    header: "Created At",
    cell: ({ cell }) => {
      const value = cell.getValue() as string;

      const formattedDate = useDateFormat(new Date(value), "DD.MM.YYYY", {
        locales: "de",
      });

      return h("div", formattedDate.value);
    },
    meta: {
      class: {
        th: "whitespace-nowrap",
      },
    },
  },
  {
    accessorKey: "affectedPatRulesCounts.totalCriticals",
    header: () =>
      h(TableHeadersIcon, {
        tooltipText: "Total Critical",
        icon: severityIcons.critical,
        iconColor: "text-severity-critical",
        iconClass: "size-5",
      }),
    cell: ({ cell }) => {
      const value = cell.getValue() as number;
      return h(CellStatCircleColumn, {
        count: value,
        severity: "critical",
      });
    },
  },
  {
    accessorKey: "affectedPatRulesCounts.totalWarnings",
    header: () =>
      h(TableHeadersIcon, {
        tooltipText: "Total Warning",
        icon: severityIcons.warning,
        iconColor: "text-severity-warning",
        iconClass: "size-5",
      }),
    cell: ({ cell }) => {
      const value = cell.getValue() as number;
      return h(CellStatCircleColumn, { count: value, severity: "warning" });
    },
  },
  {
    accessorKey: "affectedPatRulesCounts.totalOks",
    header: () =>
      h(TableHeadersIcon, {
        tooltipText: "Total Ok",
        icon: severityIcons.ok,
        iconColor: "text-severity-ok",
        iconClass: "size-5",
      }),
    cell: ({ cell }) => {
      const value = cell.getValue() as number;

      return h(CellStatCircleColumn, { count: value, severity: "ok" });
    },
  },
]);

function onSelect(row: TableRow<Pat<PatUser>>) {
  useRouter().push(
    createLink("/audit/%s/pats/%s", route.params.id as string, row.original.id)
  );
}
</script>

<template>
  <UTable
    :columns="columns"
    :data="data ?? []"
    :column-visibility="{ id: false, ...columnVisibility }"
    class="bg-white border rounded-md"
    :meta="{ class: { tr: 'cursor-pointer' } }"
    @select="onSelect"
  />
</template>
