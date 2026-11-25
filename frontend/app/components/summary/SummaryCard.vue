<script setup lang="ts">
import { navIcons } from "@/constants/navMeta";
import { categoryLabels } from "~/constants/labels";

const route = useRoute();

const props = defineProps<{
  scope: Scope;
  totalCount: number;
  criticalCount: number;
  warningCount: number;
  okCount: number;
  headerLink?: string;
  orientation?: "horizontal" | "vertical";
  ruleLinksInactive?: boolean;
}>();

const { isEnabled } = useFeatureFlags();

const rings = computed(() => {
  const criticalPercent =
    props.criticalCount / (props.totalCount ? props.totalCount : 1);
  const warningPercent =
    props.warningCount / (props.totalCount ? props.totalCount : 1);
  const okPercent = props.okCount / (props.totalCount ? props.totalCount : 1);

  return [
    {
      percent: okPercent,
      strokeClass: "stroke-lime-500",
      trackClass: okPercent > 0 ? "stroke-lime-200" : undefined,
    },
    {
      percent: warningPercent,
      strokeClass: "stroke-amber-600",
      trackClass: warningPercent > 0 ? "stroke-amber-200" : undefined,
    },
    {
      percent: criticalPercent,
      strokeClass: "stroke-red-600",
      trackClass: criticalPercent > 0 ? "stroke-red-200" : undefined,
    },
  ] as Ring[];
});

const linkTitle = computed(() => categoryLabels.get(props.scope) + " Rules");

const overViewLink = computed(() => {
  if (props.headerLink) {
    return props.headerLink;
  }
  return route.path + "/rules/" + props.scope;
});

const display = computed(() => {
  if (props.criticalCount > 0) {
    return {
      text:
        props.totalCount !== 0
          ? props.criticalCount + "/" + props.totalCount
          : 0,
      class: "text-red-600",
    };
  }
  if (props.warningCount > 0) {
    return {
      text:
        props.totalCount !== 0
          ? props.warningCount + "/" + props.totalCount
          : 0,
      class: "text-amber-600",
    };
  }
  return {
    text: props.totalCount !== 0 ? props.okCount + "/" + props.totalCount : 0,
    class: props.totalCount !== 0 ? "text-lime-600" : "text-stone-600",
  };
});

const statusText = computed(() => {
  if (props.criticalCount > 0) {
    return "Critical issues detected";
  }
  if (props.warningCount > 0) {
    return "Warning issues detected";
  }

  if (props.totalCount === 0) {
    return "No Data";
  }
  return "All good";
});

// TODO: If Project Rules and Groups Rules implemented remove this stuff.
// Note: for demo usage ( e.g. on landing page ) we need to deactivate links, so keep this.
const disabledLink = computed(() => {
  if (
    props.ruleLinksInactive !== false ||
    props.scope === "projects" ||
    props.scope === "groups"
  ) {
    return true;
  }
  return false;
});

const iconComponent = computed(() => navIcons[props.scope]);

const showComingSoonBadge = computed(() => {
  if (
    (props.scope === "projects" && !isEnabled("projects")) ||
    (props.scope === "groups" && !isEnabled("groups"))
  ) {
    return true;
  }
  return false;
});
</script>

<template>
  <UCard>
    <template #header>
      <div class="flex justify-between">
        <ULink
          :to="overViewLink"
          inactive-class="no-underline hover:text-primary!"
          class="flex items-center gap-2"
          :disabled="disabledLink"
        >
          <component :is="iconComponent" v-if="iconComponent" class="size-5" />
          {{ linkTitle }}
        </ULink>
        <ComingSoonBadge v-show="showComingSoonBadge" status="Coming Soon" />
      </div>
    </template>
    <div class="flex flex-col space-y-4">
      <div
        :class="[
          'flex flex-col gap-6 md:flex-row md:gap-4 w-full',
        ]"
      >
        <ChartRadialBar :rings class="self-center">
          <template #center>
            <span :class="['text-xl font-semibold', display.class]">{{
              display.text
            }}</span>
          </template>
        </ChartRadialBar>
        <div class="flex flex-col w-full">
          <p class="mb-4">
            Status: <span>{{ statusText }}</span>
          </p>
          <div class="flex gap-3 flex-col ml-2">
            <ULink
              :to="
                '/audit/' +
                route.params.id +
                '/rules/' +
                scope +
                '?severity=critical'
              "
              class="flex items-center gap-2"
              :disabled="disabledLink"
            >
              <StatCircle :color="criticalCount > 0 ? 'critical' : 'default'">
                <span class="font-semibold">{{ criticalCount }}</span>
              </StatCircle>
              <p :class="!disabledLink && 'hover:underline'">
                {{ linkTitle }} with critical issues
              </p>
            </ULink>
            <ULink
              :to="
                '/audit/' +
                route.params.id +
                '/rules/' +
                scope +
                '?severity=warning'
              "
              class="flex items-center gap-2"
              :disabled="disabledLink"
            >
              <StatCircle :color="warningCount > 0 ? 'warning' : 'default'">
                <span class="font-semibold">{{ warningCount }}</span>
              </StatCircle>
              <p :class="!disabledLink && 'hover:underline'">
                {{ linkTitle }} with warning
              </p>
            </ULink>
            <ULink
              :to="
                '/audit/' + route.params.id + '/rules/' + scope + '?severity=ok'
              "
              class="flex items-center gap-2"
              :disabled="disabledLink"
            >
              <StatCircle :color="okCount > 0 ? 'ok' : 'default'">
                <span class="font-semibold">{{ okCount }}</span>
              </StatCircle>
              <p :class="!disabledLink && 'hover:underline'">
                {{ linkTitle }} are ok
              </p>
            </ULink>
          </div>
        </div>
      </div>
    </div>
  </UCard>
</template>
